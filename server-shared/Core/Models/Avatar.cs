using FOMServer.Shared.Core.Enums;
using System.Runtime.InteropServices;

namespace FOMServer.Shared.Core.Models
{
    /// <summary>
    /// Represents a player avatar.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Avatar
    {
        AvatarSex sex;
        AvatarSkin skinColor;
        byte face;
        byte hair;
        byte faction;
        ushort shirt;
        ushort bottoms;
        ushort shoes;
        ushort gloves;
    }
}
