﻿using System;
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
        public string ProviderName;

        public string AppClientId;
        public string AppClientSecret;

        public string UserAccessToken;
    }

    public class AccountList
    {
        public Dictionary<string, Account> List = new Dictionary<string, Account>();
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
                AccountList o = k.Get("AccountList", new AccountList());

                foreach (KeyValuePair<string, Account> v in o.List)
                {
                    string guid = v.Key;
                    Account account = v.Value;

                    InboxAdapter a = this.Inbox.AddAdapter(guid, account.ProviderName, new InboxAdapterAppCredential { ClientId = account.AppClientId, ClientSecret = account.AppClientSecret });

                    if (account.UserAccessToken._IsFilled())
                    {
                        a.Start(new InboxAdapterUserCredential { AccessToken = account.UserAccessToken });
                    }
                }
            });
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
