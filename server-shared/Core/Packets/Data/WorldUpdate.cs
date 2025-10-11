using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Packets.Models;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Packets.Data
{
    [PacketID(PacketIdentifier.ID_WORLD_UPDATE)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WorldUpdate
    {
        public uint PlayerID;
        public PlayerUpdateModel Update;
    }
}
