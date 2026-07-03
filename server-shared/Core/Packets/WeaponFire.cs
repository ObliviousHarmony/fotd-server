using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Packets
{
    // TEMPORARY capture probe (mirrors the native WeaponFire.h). The real
    // ID_WEAPONFIRE layout is unknown; this captures the raw payload so a live
    // fire test can reveal it. Replace once decoded.
    [PacketId(PacketIdentifier.ID_WEAPONFIRE)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct WeaponFire
    {
        public ushort BitCount;
        public fixed byte Data[128];
    }
}
