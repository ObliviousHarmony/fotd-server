using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Packets.Data
{
    [PacketID(PacketIdentifier.ID_REGISTER_WORLD)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct RegisterWorld
    {
        public WorldID WorldID;
        public NetworkAddress ClientAddress;
    }
}
