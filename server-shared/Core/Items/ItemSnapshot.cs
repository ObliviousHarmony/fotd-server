using System.Runtime.CompilerServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Interop.FOMNetwork.Constants;
using FOMServer.Shared.Interop.FOMNetwork.Enums.Item;

namespace FOMServer.Shared.Core.Items
{
    public readonly struct ItemSnapshot
    {
        public readonly RecipeBalanceBuffer RecipeBalanceValues;

        public ItemSnapshot(ReadOnlySpan<byte> recipeBalanceValues)
        {
            for (var i = 0; i < BufferSizes.NumItemBalanceSliders; ++i)
            {
                RecipeBalanceValues[i] = recipeBalanceValues[i];
            }
        }

        public uint Id { get; init; }
        public ItemType Type { get; init; }
        public ItemLocationType LocationType { get; init; }
        public uint LocationId { get; init; }
        public ItemSlotType Slot { get; init; }
        public ushort Value { get; init; }
        public ushort ValueMax { get; init; }
        public ushort Durability { get; init; }
        public byte DurabilityLossFactor { get; init; }
        public ItemSecurity Security { get; init; }
        public ItemRarity Rarity { get; init; }
        public uint CreatorPlayerId { get; init; }
        public uint StolenFromPlayerId { get; init; }
        public uint Timeout { get; init; }
        public byte AttributeBonus { get; init; }
        public byte RecipeVariation { get; init; }
        public bool IsDestroyed { get; init; }

        [InlineArray(BufferSizes.NumItemBalanceSliders)]
        public struct RecipeBalanceBuffer
        {
            private byte _element0;
        }
    }
}
