using FOMServer.Shared.Core.Dtos;
using FOMServer.Shared.Interop.FOMNetwork.Constants;

namespace FOMServer.Shared.Core.Repositories
{
    public interface IPlayerRepository
    {
        PlayerDto? Create(
            uint id,
            string name,
            string biography,
            AvatarConstants.Sex sex,
            AvatarConstants.Race race,
            ushort face,
            ushort hair
        );

        PlayerDto? GetById(uint id);

        PlayerDto? GetByName(string name);

        string? GetBiography(uint id);
    }
}
