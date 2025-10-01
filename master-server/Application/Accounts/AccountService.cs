using FOMServer.Master.Core.Accounts;
using FOMServer.Master.Core.Repositories;
using FOMServer.Shared.Core.FOMPacket.Models;
using System.Collections.Concurrent;

namespace FOMServer.Master.Application.Accounts
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository accountRepository;

        private readonly ConcurrentDictionary<uint, Account> loggedInAccounts;
        private readonly ConcurrentDictionary<NetworkAddress, Account> loggedInAccountsByAddress;

        public AccountService(IAccountRepository accountRepository)
        {
            this.accountRepository = accountRepository;
            loggedInAccounts = new ConcurrentDictionary<uint, Account>();
            loggedInAccountsByAddress = new ConcurrentDictionary<NetworkAddress, Account>();
        }

        public Account? Get(uint id)
        {
            if (!loggedInAccounts.TryGetValue(id, out var account))
                return null;
            return account;
        }

        public Account? Get(NetworkAddress clientAddress)
        {
            if (!loggedInAccountsByAddress.TryGetValue(clientAddress, out var account))
                return null;
            return account;
        }

        public Account? Login(string username, string password, NetworkAddress clientAddress)
        {
            var dto = accountRepository.TryLogin(username, password);
            if (dto == null)
                return null;

            if (loggedInAccounts.ContainsKey(dto.id))
                return null;

            if (loggedInAccountsByAddress.ContainsKey(clientAddress))
                return null;

            var account = new Account
            {
                ClientAddress = clientAddress,
                ID = dto.id,
                Username = dto.username
            };

            if (!loggedInAccounts.TryAdd(dto.id, account))
                return null;

            if (!loggedInAccountsByAddress.TryAdd(clientAddress, account))
            {
                loggedInAccounts.TryRemove(dto.id, out _);
                return null;
            }

            return account;
        }

        public bool Logout(Account account)
        {
            if (!loggedInAccounts.ContainsKey(account.ID))
                return false;

            if (!loggedInAccounts.TryRemove(account.ID, out _))
                return false;

            loggedInAccountsByAddress.TryRemove(account.ClientAddress, out _);

            accountRepository.Logout(account.ID);

            return true;
        }
    }
}
