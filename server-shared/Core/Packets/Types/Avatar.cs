using System;
using System.Net;
using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;

namespace FOMServer.Shared.Core.Packets.Types
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct Avatar
    {
        public AvatarSex Sex;
        public AvatarRace Race;
        public ushort Face;
        public ushort Hair;

        public ushort FactionID;
        public ushort RankID;
        public ushort LegacyFactionID;

        public ushort Shirt;
        public ushort Bottoms;
        public ushort Shoes;

        public fixed ushort EquipmentSlots[(int)EquipmentSlot.NUM_EQUIPMENT_SLOTS];
    }
}
