using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket.Metadata;

namespace FOMServer.Shared.Core.FOMPacket.Data
{
    [PacketID(PacketIdentifier.ID_CONNECTION_REQUEST_ACCEPTED)]
    [PacketID(PacketIdentifier.ID_CONNECTION_ATTEMPT_FAILED)]
    [PacketID(PacketIdentifier.ID_ALREADY_CONNECTED)]
    [PacketID(PacketIdentifier.ID_NEW_INCOMING_CONNECTION)]
    [PacketID(PacketIdentifier.ID_NO_FREE_INCOMING_CONNECTIONS)]
    [PacketID(PacketIdentifier.ID_DISCONNECTION_NOTIFICATION)]
    [PacketID(PacketIdentifier.ID_CONNECTION_LOST)]
    [PacketID(PacketIdentifier.ID_RSA_PUBLIC_KEY_MISMATCH)]
    [PacketID(PacketIdentifier.ID_CONNECTION_BANNED)]
    [PacketID(PacketIdentifier.ID_INVALID_PASSWORD)]
    [PacketID(PacketIdentifier.ID_ALREADY_CONNECTED)]
    [PacketID(PacketIdentifier.ID_MODIFIED_PACKET)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RakNetPacket { }
}
