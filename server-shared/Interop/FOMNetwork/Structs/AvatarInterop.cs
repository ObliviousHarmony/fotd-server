using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Constants;

namespace FOMServer.Shared.Interop.FOMNetwork.Structs
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AvatarInterop
    {
        public AvatarConstants.Sex Sex;
        public AvatarConstants.Race Race;
        public ushort Face;
        public ushort Hair;

        public ushort FactionId;
        public ushort RankId;
        public ushort Unknown1;
        public ushort LegacyFactionId;

        public ushort Shirt;
        public ushort Bottoms;
        public ushort Shoes;
        public ushort Hat;
        public ushort Head;
        public ushort Eyes;
        public ushort Shoulder;
        public ushort Arms;
        public ushort Torso;
        public ushort Back;
        public ushort Legs;
        public ushort Hands;

        public byte IsCommander;
        public byte Unknown2;
        public byte Unknown3;
        public byte IsGroupLeader;
    }
}
