using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;

namespace FOMServer.Shared.Core.Packets.Types
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct ItemBase
    {
        public ItemType Type;
        public ushort Value;
        public ushort MaxDurability;
        public ushort Durability;
        public byte DurabilityLossFactor;
        public ItemSecurity Security;
        public uint CreatorPlayerId;
        public uint Timeout;
        public uint StolenFromPlayerId;
        public byte Classification;
        public ItemQuality Quality;
        public byte AttributeBonus;
        public fixed byte BalanceValues[4];
    }
}
