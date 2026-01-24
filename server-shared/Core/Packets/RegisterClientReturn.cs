using System.Runtime.InteropServices;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Packets
{
    [PacketID(Enums.PacketIdentifier.ID_REGISTER_CLIENT_RETURN)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RegisterClientReturn
    {
        public byte WorldID;
        public uint PlayerID;
        public StatusCode Status;

        public enum StatusCode : byte
        {
            Success = 1,            // REGISTER_CLIENT_RETURN_SUCCESS
            WorldFull = 4,          // REGISTER_CLIENT_RETURN_WORLD_FULL
            InvalidWorldFile = 5,   // REGISTER_CLIENT_RETURN_INVALID_WORLD_FILE
        }
    }
}
