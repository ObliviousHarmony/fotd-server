using System.Runtime.InteropServices;
using FOMServer.Shared.Interop.FOMNetwork.Constants;
using FOMServer.Shared.Interop.FOMNetwork.Enums.Item;

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

        public ItemType Shirt;
        public ItemType Bottoms;
        public ItemType Shoes;
        public ItemType Hat;
        public ItemType Head;
        public ItemType Eyes;
        public ItemType Shoulder;
        public ItemType Arms;
        public ItemType Torso;
        public ItemType Back;
        public ItemType Legs;
        public ItemType Hands;

        public byte IsCommander;
        public byte Unknown2;
        public byte Unknown3;
        public byte IsGroupLeader;
    }
}
