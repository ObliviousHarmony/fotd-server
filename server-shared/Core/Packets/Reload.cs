using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Packets
{
    // TEMPORARY capture probe (mirrors the native Reload.h). See WeaponFire.cs.
    [PacketId(PacketIdentifier.ID_RELOAD)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct Reload
    {
        public ushort BitCount;
        public fixed byte Data[128];
    }
}
