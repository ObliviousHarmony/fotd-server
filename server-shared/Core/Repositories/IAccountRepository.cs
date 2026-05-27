using FOMServer.Shared.Core.Dtos;

namespace FOMServer.Shared.Core.Repositories
{
    public interface IAccountRepository
    {
        AccountDto? GetById(uint id);

        AccountDto? GetByUsername(string username);
    }
}
