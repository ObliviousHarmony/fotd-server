using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;

namespace FOMServer.Shared.Core.Packets.Models
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
        public byte ShowArmor;
        public ushort ArmorHead;     // ShowArmor == 1
        public ushort ArmorGlasses;  // ShowArmor == 1
        public ushort ArmorShoulder; // ShowArmor == 1
        public ushort ArmorArm;      // ShowArmor == 1
        public ushort ArmorTorso;    // ShowArmor == 1
        public ushort ArmorLeg;      // ShowArmor == 1
        public byte Rank;
    }
}
