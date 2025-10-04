using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.FOMPacket.Data.RakNetPackets
{
    [PacketID(PacketIdentifier.ID_RSA_PUBLIC_KEY_MISMATCH)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RSAPublicKeyMismatch { }
}
