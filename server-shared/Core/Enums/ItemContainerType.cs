namespace FOMServer.Shared.Core.Enums
{
    public enum ItemContainerType : byte
    {
        None = 0, // ITEM_CONTAINER_NONE

        Inventory = 1, // ITEM_CONTAINER_INVENTORY
        Equipment = 2, // ITEM_CONTAINER_EQUIPMENT
        Weapons = 3, // ITEM_CONTAINER_WEAPONS

        Quickslots = 5, // ITEM_CONTAINER_QUICKSLOTS

        Destroy = 8, // ITEM_CONTAINER_DESTROY
    }
}
