using System.Runtime.InteropServices;
using FOMServer.Shared.Interop.FOMNetwork.Constants;
using FOMServer.Shared.Interop.FOMNetwork.Enums.Item;

namespace FOMServer.Shared.Interop.FOMNetwork.Structs.Item
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct ItemBaseInterop
    {
        public ItemType Type;
        public ushort Value;
        public ushort ValueMax;
        public ushort Durability;
        public byte DurabilityLossFactor;
        public ItemSecurity Security;
        public uint CreatorPlayerId;
        public uint StolenFromPlayerId;
        public uint Timeout;
        public byte RecipeVariation;
        public ItemRarity Rarity;
        public byte AttributeBonus;
        public fixed byte RecipeBalanceValues[BufferSizes.NumItemBalanceSliders];
    }
}
