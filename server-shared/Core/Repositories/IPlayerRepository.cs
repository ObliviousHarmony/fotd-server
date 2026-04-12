using FOMServer.Shared.Core.Dtos;
using FOMServer.Shared.Core.Enums;

namespace FOMServer.Shared.Core.Repositories
{
    public interface IPlayerRepository
    {
        PlayerDto? Create(uint id, string name, string biography, AvatarSex sex, AvatarRace race, ushort face, ushort hair);
        PlayerDto? GetByID(uint id);
        PlayerDto? GetByName(string name);
        string? GetBiography(uint id);

    }
}
