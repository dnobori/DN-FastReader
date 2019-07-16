using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using IPA.Cores.Basic;
using IPA.Cores.Helper.Basic;
using static IPA.Cores.Globals.Basic;

namespace DN_FastReader
{
    public class Account
    {
        public string Guid;

        public string ProviderName;

        public string AppClientId;
        public string AppClientSecret;

        public string UserAccessToken;

        public string AccountInfoStr;
    }

    public class AccountList
    {
        public List<Account> List = new List<Account>();
    }

    public class FastReader : IDisposable
    {
        readonly Inbox Inbox;

        readonly HiveData<HiveKeyValue> AccountsHive;

        public FastReader()
        {
            this.Inbox = new Inbox();

            this.Inbox.StateChangeEventListener.RegisterCallback(async (caller, type, state) => { UpdatedCallback(); await Task.CompletedTask; });

            this.AccountsHive = Hive.LocalAppSettingsEx["Accounts"];

            this.AccountsHive.AccessData(true, k =>
            {
                var initial = new AccountList();

                //initial.List.Add(new Account { AppClientId = "123", Guid = "456", });

                AccountList o = k.Get("AccountList", initial);

                foreach (Account account in o.List)
                {
                    InboxAdapter a = this.Inbox.AddAdapter(account.Guid, account.ProviderName, new InboxAdapterAppCredential { ClientId = account.AppClientId, ClientSecret = account.AppClientSecret });

                    if (account.UserAccessToken._IsFilled())
                    {
                        a.Start(new InboxAdapterUserCredential { AccessToken = account.UserAccessToken });
                    }
                }
            });
        }

        public AccountList GetAccountList()
        {
            InboxAdapter[] adapters = Inbox.EnumAdapters();

            AccountList ret = new AccountList();

            foreach (InboxAdapter ad in adapters)
            {
                Account ac = new Account
                {
                    Guid = ad.Guid,
                    ProviderName = ad.AdapterName,
                    AppClientId = ad.AppCredential.ClientId,
                    AppClientSecret = ad.AppCredential.ClientSecret,
                    UserAccessToken = ad.UserCredential?.AccessToken,
                    AccountInfoStr = ad.AccountInfoStr,
                };

                ret.List.Add(ac);
            }

            return ret;
        }

        public void Dispose()
        {
            Inbox._DisposeSafe();
        }

        ulong currentVersion = 0;

        InboxMessageBox currentBox = new InboxMessageBox();

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
    }
}
