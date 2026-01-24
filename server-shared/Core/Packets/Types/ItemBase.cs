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
        public uint CreatorPlayerID;
        public uint Timeout;
        public uint StolenFromPlayerID;
        public byte AttributeBonus;
        public ItemQuality Quality;
        public byte Classification;
        public fixed byte BalanceValues[4];
    }
}
