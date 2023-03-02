using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DN_FastReader.Models;

using IPA.Cores.Basic;
using IPA.Cores.Helper.Basic;
using static IPA.Cores.Globals.Basic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DN_FastReader
{
    public class Account
    {
        public string Guid { get; set; } = null!;

        public string ProviderName { get; set; } = null!;

        public string AppClientId { get; set; } = null!;
        public string AppClientSecret { get; set; } = null!;

        public string UserAccessToken { get; set; } = null!;

        public string AccountInfoStr { get; set; } = null!;

        public bool IsStarted { get; set; }

        public string ErrorStr { get; set; } = null!;

        public string DeleteUrl { get; set; } = null!;
    }

    public class AccountSetting
    {
        public string Guid = null!;

        public string ProviderName = null!;

        public string AppClientId = null!;
        public string AppClientSecret = null!;

        public string UserAccessToken = null!;
    }

    public class AccountSettingList
    {
        public List<AccountSetting> List = new List<AccountSetting>();
    }

#pragma warning disable CA1063 // Implement IDisposable Correctly
    public class FastReader : IDisposable
#pragma warning restore CA1063 // Implement IDisposable Correctly
    {
        public Inbox Inbox { get; }

        readonly HiveData<HiveKeyValue> AccountsHive;

        public FastReader()
        {
            try
            {
                bool ignoreSslCert = false;

                this.AccountsHive = Hive.LocalAppSettingsEx["Accounts"];

                this.AccountsHive.AccessData(true, k =>
                {
                    ignoreSslCert = k.GetBool("IgnoreSslCert");
                });

                this.Inbox = new Inbox(new InboxOptions(recordRealtimeTextLog: true, ignoreSslCert: ignoreSslCert));

                this.Inbox.StateChangeEventListener.RegisterCallback(async (caller, type, state, args) => { UpdatedCallback(); await Task.CompletedTask; });

                this.AccountsHive.AccessData(true, k =>
                {
                    var initial = new AccountSettingList();

                    AccountSettingList o = k.Get("AccountList", initial)!;

                    foreach (AccountSetting account in o.List)
                    {
                        if (account.ProviderName._IsSamei(Consts.InboxProviderNames.Slack_Old))
                        {
                            account.ProviderName = Consts.InboxProviderNames.Slack_App;
                        }

                        InboxAdapter a = this.Inbox.AddAdapter(account.Guid, account.ProviderName, new InboxAdapterAppCredential { ClientId = account.AppClientId, ClientSecret = account.AppClientSecret });

                        if (account.UserAccessToken._IsFilled())
                        {
                            a.Start(new InboxAdapterUserCredential { AccessToken = account.UserAccessToken });
                        }
                        else if (account.ProviderName._IsSamei(Consts.InboxProviderNames.Slack_User))
                        {
                            // Special: Slack legacy tokens
                            a.Start(new InboxAdapterUserCredential());
                        }
                    }
                });
            }
            catch
            {
                this._DisposeSafe();
                throw;
            }
        }

        public void Dispose()
        {
            Inbox._DisposeSafe();
        }

        public string[] GetProviderNamesList()
        {
            return Inbox.GetProviderNameList();
        }

        public Account[] GetAccountList()
        {
            InboxAdapter[] adapters = Inbox.EnumAdapters();

            List<Account> ret = new List<Account>();

            foreach (InboxAdapter ad in adapters)
            {
                Account ac = new Account
                {
                    Guid = ad.Guid,
                    ProviderName = ad.AdapterName,
                    AppClientId = ad.AppCredential.ClientId!,
                    AppClientSecret = ad.AppCredential.ClientSecret!,
                    UserAccessToken = ad.UserCredential?.AccessToken!,
                    AccountInfoStr = ad.AccountInfoStr!,
                    ErrorStr = ad.LastError?.Message!,
                    IsStarted = ad.IsStarted,
                };

                ret.Add(ac);
            }
            
            return ret.ToArray();
        }

        ulong currentVersion = 0;

        InboxMessageBox currentBox = new InboxMessageBox(true);

        void UpdatedCallback()
        {
            InboxMessageBox box = this.Inbox.GetMessageBox();

            currentVersion = box.Version;
            this.currentBox = box;
        }

        public int GetCurrentVersion()
        {
            return (int)(this.currentVersion._RawReadValueUInt32() & 0x7ffffffff);
        }

        public InboxMessageBox GetCurrentBox()
        {
            return this.currentBox;
        }

        public IEnumerable<SelectListItem> GetProvidersDropDownList(string currentSelected)
        {
            List<SelectListItem> ret = new List<SelectListItem>();

            string[] providers = Inbox.GetProviderNameList();

            providers._DoForEach(x => ret.Add(new SelectListItem(x._ReplaceStr("_", " "), x, currentSelected._IsSamei(x))));

            return ret;
        }

        public string AddAccount(Account a)
        {
            string guid = Str.NewGuid();

            InboxAdapter adapter = Inbox.AddAdapter(guid, a.ProviderName, new InboxAdapterAppCredential { ClientId = a.AppClientId, ClientSecret = a.AppClientSecret });

            SaveSettingsFile();

            return guid;
        }

        public void SaveSettingsFile()
        {
            this.AccountsHive.AccessData(true, k =>
            {
                AccountSettingList o = new AccountSettingList();

                foreach (var adapter in this.Inbox.EnumAdapters())
                {
                    o.List.Add(new AccountSetting
                    {
                        Guid = adapter.Guid,
                        ProviderName = adapter.AdapterName,
                        AppClientId = adapter.AppCredential?.ClientId!,
                        AppClientSecret = adapter.AppCredential?.ClientSecret!,
                        UserAccessToken = adapter.UserCredential?.AccessToken!
                    });
                }

                k.Set("AccountList", o);
            });
        }

        public InboxAdapter GetAdapter(string guid)
        {
            InboxAdapter adapter = Inbox.EnumAdapters().Single(x => x.Guid._IsSamei(guid));

            return adapter;
        }

        public void DeleteAdapter(string guid)
        {
            Inbox.DeleteAdapter(guid);
        }

        public int GetErrorCount()
        {
            int ret = 0;
            foreach (var a in Inbox.EnumAdapters())
            {
                if (a.LastError != null)
                {
                    ret++;
                }
            }
            return ret;
        }
    }
}
