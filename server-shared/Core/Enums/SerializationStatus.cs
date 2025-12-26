namespace FOMServer.Shared.Core.Enums
{
    /// <summary>
    /// Status code prefixed to each packet in the buffer.
    /// Used to indicate whether deserialization succeeded or failed.
    /// </summary>
    public enum SerializationStatus : byte
    {
        Success = 0, // SERIALIZATION_SUCCESS
        ReadError = 1, // SERIALIZATION_READ_ERROR
        UnhandledPacket = 2, // SERIALIZATION_UNHANDLED_PACKET
    }
}
