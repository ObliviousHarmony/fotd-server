using FOMServer.Shared.Core.DTOs;

namespace FOMServer.Shared.Core.Players
{
    public interface IPlayerRepositoryBase
    {
        PlayerDTO? GetByID(uint id);
    }
}
