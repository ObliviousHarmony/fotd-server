using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Persistence;

namespace FOMServer.Shared.Core.Items
{
    public interface IItemLocation
    {
        ItemLocationRef LocationRef { get; }

        IEnumerable<ItemContainer> GetItemContainers();

        ItemContainer? GetItemContainer(ItemSlotType slotType);
    }

    public readonly record struct ItemLocationRef(
        ItemLocationType Type,
        uint Id,
        IPersistable? Persistable)
    {

        public bool IsPlayer()
        {
            return Type is ItemLocationType.Inventory;
        }

        public bool IsPlayer(uint id )
        {
            return IsPlayer() && id == Id;
        }
    }
}
