using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Packets
{
    [PacketId(PacketIdentifier.ID_UPDATE)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Update
    {
        public Types.WorldUpdate WorldUpdate;
    }
}
