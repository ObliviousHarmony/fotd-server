using FOMServer.Shared.Core.DTOs;

namespace FOMServer.Shared.Core.Repositories
{
    public interface IAccountRepository
    {
        AccountDTO? GetByID(uint id);
        AccountDTO? GetByUsername(string username);
    }
}
