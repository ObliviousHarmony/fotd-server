using System.Runtime.InteropServices;

namespace FOMServer.Shared.Core.Models.FOMData
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
        [FieldOffset(0)] public RakNetPacket rakNetPacket;
        [FieldOffset(0)] public ReadPacketError readError;
        [FieldOffset(0)] public LoginRequest loginRequest;
        [FieldOffset(0)] public LoginRequestReturn loginRequestReturn;
        [FieldOffset(0)] public Login login;
        [FieldOffset(0)] public LoginReturn loginReturn;
        [FieldOffset(0)] public CheckName checkName;
        [FieldOffset(0)] public CheckNameReturn checkNameReturn;
        [FieldOffset(0)] public CreateCharacter createCharacter;
    }
}
