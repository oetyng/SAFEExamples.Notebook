using System;
using System.Collections.Generic;
using System.Text;

namespace SAFE.DotNET.Auth
{
    internal class AccountStore
    {
        Dictionary<string, List<Account>> _accounts = new Dictionary<string, List<Account>>();

        internal static AccountStore Create()
        {
            return new AccountStore();
        }

        internal void Save(Account acctInfo, string appName)
        {
            if (!_accounts.ContainsKey(appName))
                _accounts[appName] = new List<Account>();
            if (!_accounts[appName].Contains(acctInfo))
                _accounts[appName].Add(acctInfo);
        }

        internal void Delete(Account acctInfo, string appName)
        {
            if (!_accounts.ContainsKey(appName))
                return;
            if (!_accounts[appName].Contains(acctInfo))
                return;
            _accounts[appName].Remove(acctInfo);
        }

        internal List<Account> FindAccountsForService(string appName)
        {
            if (!_accounts.ContainsKey(appName))
                return new List<Account>();
            
            return _accounts[appName];
        }
    }

    internal class Account
    {
        public string Username { get; internal set; }
        public Dictionary<string, string> Properties { get; internal set; }
    }
}
