namespace FOMServer.Shared.Core.Enums
{
    public enum ItemContainerType : byte
    {
        None = 0, // ITEM_CONTAINER_NONE

        Inventory = 1, // ITEM_CONTAINER_INVENTORY
        Equipment = 2, // ITEM_CONTAINER_EQUIPMENT
        Weapons = 3, // ITEM_CONTAINER_WEAPONS
        NanomachineAugmentations = 4, // ITEM_CONTAINER_NANOMACHINE_AUGMENTATIONS
        Quickslots = 5, // ITEM_CONTAINER_QUICKSLOTS
        Storage = 6, // ITEM_CONTAINER_STORAGE
        ActiveConsumables = 7, // ITEM_CONTAINER_ACTIVE_CONSUMABLES
        Destroy = 8, // ITEM_CONTAINER_DESTROY

        Loot = 11, // ITEM_CONTAINER_LOOT

        TransportStorage = 13, // ITEM_CONTAINER_TRANSPORT_STORAGE

        TransportBuyback = 17, // ITEM_CONTAINER_TRANSPORT_BUYBACK

        NUM_ITEM_CONTAINERS = 18,
    }
}
