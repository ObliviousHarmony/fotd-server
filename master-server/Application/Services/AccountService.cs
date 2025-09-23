using FOMServer.Master.Core.DTOs;
using FOMServer.Master.Core.Interfaces;
using FOMServer.Master.Core.Models;
using System.Collections.Concurrent;

namespace FOMServer.Master.Application.Services
{
    internal class AccountService : IAccountService
    {
        private readonly IAccountRepository accountRepository;

        private readonly ConcurrentDictionary<uint, Account> loggedInAccounts;

        public AccountService(IAccountRepository accountRepository)
        {
            this.accountRepository = accountRepository;
            this.loggedInAccounts = new ConcurrentDictionary<uint, Account>();
        }

        public Account? Get(uint id)
        {
            if (!loggedInAccounts.TryGetValue(id, out var account))
                return null;
            return account;
        }

        public Account? Login(string username, string password)
        {
            var dto = accountRepository.TryLogin(username, password);
            if (dto == null)
                return null;

            if (loggedInAccounts.ContainsKey(dto.id))
                return null;

            var account = new Account { ID = dto.id, Username = dto.username };
            if (!loggedInAccounts.TryAdd(dto.id, account))
                return null;

            return account;
        }

        public bool Logout(Account account)
        {
            if (!loggedInAccounts.ContainsKey(account.ID))
                return false;

            if (!loggedInAccounts.TryRemove(account.ID, out _))
                return false;

            return true;
        }
    }
}
