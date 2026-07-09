using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Persistence;

namespace FOMServer.Shared.Core.Items
{
    public interface IItemLocation
    {
        ItemLocationRef Location { get; }

        ItemContainer? GetItemContainer(ItemContainerType type, ItemSlotType slotType);
    }

    public readonly record struct ItemLocationRef(
        ItemLocationType Type,
        uint Id,
        IPersistable? Persistable);
}
