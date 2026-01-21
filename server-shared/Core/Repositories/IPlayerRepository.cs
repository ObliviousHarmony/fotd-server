using FOMServer.Shared.Core.DTOs;
using FOMServer.Shared.Core.Enums;

namespace FOMServer.Shared.Core.Repositories
{
    public interface IPlayerRepository
    {
        PlayerDTO? GetByID(uint id);
        PlayerDTO? GetByName(string name);
        string? GetBiography(uint id);
        PlayerDTO? Create(uint id, string name, string biography, AvatarSex sex, AvatarRace race, ushort face, ushort hair);
    }
}
