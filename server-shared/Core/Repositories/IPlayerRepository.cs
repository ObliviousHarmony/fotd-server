using FOMServer.Shared.Core.Constants;
using FOMServer.Shared.Core.Dtos;

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
