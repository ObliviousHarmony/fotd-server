using FOMServer.Shared.Core.Dtos;
using FOMServer.Shared.Interop.FOMNetwork.Constants;

namespace FOMServer.Shared.Core.Repositories
{
    public interface IPlayerRepository
    {
        uint? Create(PlayerDto player, string biography);

        PlayerDto? GetById(uint id);

        PlayerDto? GetByName(string name);

        string? GetBiography(uint id);
    }
}
