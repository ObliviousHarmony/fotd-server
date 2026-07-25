using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Interop.FOMNetwork.Enums;
using FOMServer.Shared.Interop.FOMNetwork.Enums.Item;

namespace FOMServer.Shared.Core.Dtos
{
    public record ItemDto
    {
        public uint id { get; init; }

        public ItemType type { get; init; }

        public ItemLocationType location_type { get; init; }

        public uint location_id { get; init; }

        public ItemSlotType slot { get; init; }

        public ushort value { get; init; }

        public ushort value_max { get; init; }

        public ushort durability { get; init; }

        public byte durability_loss_factor { get; init; }

        public ItemSecurity security { get; init; }

        public ItemRarity rarity { get; init; }

        public uint creator_player_id { get; init; }

        public uint stolen_from_player_id { get; init; }

        public uint timeout { get; init; }

        public byte attribute_bonus { get; init; }

        public byte recipe_variation { get; init; }

        public byte recipe_balance_1 { get; init; }

        public byte recipe_balance_2 { get; init; }

        public byte recipe_balance_3 { get; init; }

        public byte recipe_balance_4 { get; init; }
    }
}
