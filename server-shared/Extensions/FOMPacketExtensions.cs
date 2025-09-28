using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Models;
using FOMServer.Shared.Core.Models.FOMData;

namespace FOMServer.Shared.Extensions
{
    public static class FOMPacketExtensions
    {
        /// <summary>
        /// Extracts strongly typed data from the packet and validates the ID.
        /// </summary>
        public static TPacket GetData<TPacket>(this FOMPacket packet)
        {
            switch (packet.ID)
            {
                case PacketIdentifier.ID_CONNECTION_REQUEST_ACCEPTED when typeof(TPacket) == typeof(RakNetPacket):
                case PacketIdentifier.ID_CONNECTION_ATTEMPT_FAILED when typeof(TPacket) == typeof(RakNetPacket):
                case PacketIdentifier.ID_ALREADY_CONNECTED when typeof(TPacket) == typeof(RakNetPacket):
                case PacketIdentifier.ID_NEW_INCOMING_CONNECTION when typeof(TPacket) == typeof(RakNetPacket):
                case PacketIdentifier.ID_NO_FREE_INCOMING_CONNECTIONS when typeof(TPacket) == typeof(RakNetPacket):
                case PacketIdentifier.ID_DISCONNECTION_NOTIFICATION when typeof(TPacket) == typeof(RakNetPacket):
                case PacketIdentifier.ID_CONNECTION_LOST when typeof(TPacket) == typeof(RakNetPacket):
                case PacketIdentifier.ID_RSA_PUBLIC_KEY_MISMATCH when typeof(TPacket) == typeof(RakNetPacket):
                case PacketIdentifier.ID_CONNECTION_BANNED when typeof(TPacket) == typeof(RakNetPacket):
                case PacketIdentifier.ID_INVALID_PASSWORD when typeof(TPacket) == typeof(RakNetPacket):
                case PacketIdentifier.ID_MODIFIED_PACKET when typeof(TPacket) == typeof(RakNetPacket):
                    return (TPacket)(object)packet.Data.rakNetPacket;

                case PacketIdentifier.ID_FOM_PACKET_READ_ERROR when typeof(TPacket) == typeof(ReadPacketError):
                    return (TPacket)(object)packet.Data.readError;
                case PacketIdentifier.ID_LOGIN_REQUEST when typeof(TPacket) == typeof(LoginRequest):
                    return (TPacket)(object)packet.Data.loginRequest;
                case PacketIdentifier.ID_LOGIN_REQUEST_RETURN when typeof(TPacket) == typeof(LoginRequestReturn):
                    return (TPacket)(object)packet.Data.loginRequestReturn;
                case PacketIdentifier.ID_LOGIN when typeof(TPacket) == typeof(Login):
                    return (TPacket)(object)packet.Data.login;
                case PacketIdentifier.ID_LOGIN_RETURN when typeof(TPacket) == typeof(LoginReturn):
                    return (TPacket)(object)packet.Data.loginReturn;
                case PacketIdentifier.ID_CHECK_NAME when typeof(TPacket) == typeof(CheckName):
                    return (TPacket)(object)packet.Data.checkName;
                case PacketIdentifier.ID_CHECK_NAME_RETURN when typeof(TPacket) == typeof(CheckNameReturn):
                    return (TPacket)(object)packet.Data.checkNameReturn;
                case PacketIdentifier.ID_CREATE_CHARACTER when typeof(TPacket) == typeof(CreateCharacter):
                    return (TPacket)(object)packet.Data.createCharacter;

                default:
                    throw new InvalidOperationException(
                        $"Packet ID {packet.ID} does not match the requested type {typeof(TPacket).Name}");
            }
        }
    }
}
