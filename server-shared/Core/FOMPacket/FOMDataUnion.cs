using System.Runtime.InteropServices;
using FOMServer.Shared.Core.FOMPacket.Data;
using FOMServer.Shared.Core.FOMPacket.Data.RakNetPackets;

namespace FOMServer.Shared.Core.FOMPacket
{
    /// <summary>
    /// Represents a union of possible packet data types.
    /// </summary>
    /// <remarks>
    /// This structure uses explicit layout to allow overlapping fields, enabling it to store different kinds
    /// of packet data. Only one of the fields should be accessed at a time, as they share the same memory space.
    /// </remarks>
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct FOMDataUnion
    {
        [FieldOffset(0)] public AlreadyConnected AlreadyConnected;
        [FieldOffset(0)] public ConnectionAttemptFailed ConnectionAttemptFailed;
        [FieldOffset(0)] public ConnectionBanned ConnectionBanned;
        [FieldOffset(0)] public ConnectionLost ConnectionLost;
        [FieldOffset(0)] public ConnectionRequestAccepted ConnectionRequestAccepted;
        [FieldOffset(0)] public DisconnectionNotification DisconnectionNotification;
        [FieldOffset(0)] public InvalidPassword InvalidPassword;
        [FieldOffset(0)] public ModifiedPacket ModifiedPacket;
        [FieldOffset(0)] public NewIncomingConnection NewIncomingConnection;
        [FieldOffset(0)] public NoFreeIncomingConnections NoFreeIncomingConnections;
        [FieldOffset(0)] public RSAPublicKeyMismatch RSAPublicKeyMismatch;
        [FieldOffset(0)] public ReadPacketError ReadError;
        [FieldOffset(0)] public LoginRequest LoginRequest;
        [FieldOffset(0)] public LoginRequestReturn LoginRequestReturn;
        [FieldOffset(0)] public Login Login;
        [FieldOffset(0)] public LoginReturn LoginReturn;
        [FieldOffset(0)] public CheckName CheckName;
        [FieldOffset(0)] public CheckNameReturn CheckNameReturn;
        [FieldOffset(0)] public CreateCharacter CreateCharacter;
        [FieldOffset(0)] public RegisterWorld RegisterWorld;
        [FieldOffset(0)] public WorldOverview WorldOverview;
        [FieldOffset(0)] public WorldOverviewReturn WorldOverviewReturn;
        [FieldOffset(0)] public WorldLogin WorldLogin;
        [FieldOffset(0)] public WorldLoginReturn WorldLoginReturn;
        [FieldOffset(0)] public PlayerEnteringWorld PlayerEnteringWorld;
        [FieldOffset(0)] public PlayerEnteringWorldReturn PlayerEnteringWorldReturn;
    }
}
