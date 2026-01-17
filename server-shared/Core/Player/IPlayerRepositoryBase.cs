using FOMServer.Shared.Core.DTOs;

namespace FOMServer.Shared.Core.Player
{
    public interface IPlayerRepositoryBase
    {
        PlayerDTO? GetByID(uint id);
    }
}
