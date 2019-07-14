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

        readonly HiveData<HiveKeyValue> Accounts;

        public FastReader()
        {
            this.Inbox = new Inbox();

            this.Accounts = Hive.LocalAppSettingsEx["Accounts"];

            this.Accounts.AccessData(true, k =>
            {
                AccountList o = k.Get<AccountList>("AccountList", new AccountList());

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
    }
}
