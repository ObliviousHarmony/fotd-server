using FOMServer.Shared.Core.Dtos;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Interop.FOMNetwork.Enums;
using FOMServer.Shared.Interop.FOMNetwork.Enums.Item;

namespace FOMServer.Shared.Core.Repositories
{
    public interface IItemRepository
    {
        uint? Create(ItemDto item, WorldId? worldId = null);

        ItemDto? GetById(uint id);

        IReadOnlyDictionary<uint, ItemDto> GetPlayerItems(uint playerId, WorldId worldId);
    }
}
