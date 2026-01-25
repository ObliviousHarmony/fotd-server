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
        public byte Unknown1; // 6 bits on wire
        public ushort LegacyFactionID;

        public fixed ushort EquipmentSlots[(int)EquipmentSlot.NUM_EQUIPMENT_SLOTS];

        public byte IsCommander;   // 1 bit on wire
        public byte Unknown2;      // 1 bit on wire
        public byte Unknown3;      // 1 bit on wire
        public byte IsGroupLeader; // 1 bit on wire
    }
}
