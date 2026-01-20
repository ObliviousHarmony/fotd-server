using FOMServer.Shared.Core.DTOs;

namespace FOMServer.Shared.Core.Repositories
{
    public interface IPlayerRepository
    {
        PlayerDTO? GetByID(uint id);
        PlayerDTO? GetByName(string name);
        string? GetBiography(uint id);
    }
}
