using FOMServer.Shared.Core.Enums;
using System.Runtime.InteropServices;

namespace FOMServer.Shared.Core.FOMPacket.Models
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AvatarModel
    {
        public AvatarSex Sex;
        public AvatarSkin SkinColor;
        public byte Face;
        public byte Hair;
        public Faction Faction;
        public ushort Shirt;
        public ushort Bottoms;
        public ushort Shoes;
        public ushort Gloves;
    }
}
