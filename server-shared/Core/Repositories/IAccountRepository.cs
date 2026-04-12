using FOMServer.Shared.Core.Dtos;

namespace FOMServer.Shared.Core.Repositories
{
    public interface IAccountRepository
    {
        AccountDto? GetByID(uint id);
        AccountDto? GetByUsername(string username);
    }
}
