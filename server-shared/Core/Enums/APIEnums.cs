namespace FOMServer.Shared.Core.Enums
{
    /// <summary>
    /// The priority for a packet to be sent with.
    /// </summary>
    public enum PacketPriority : byte
    {
        High = 1, // HIGH_PRIORITY,
        Medium = 2, // MEDIUM_PRIORITY
        Low = 3 // LOW_PRIORITY
    }

    /// <summary>
    /// How reliable a packet should be when sent.
    /// </summary>
    public enum PacketReliability : byte
    {
        Unreliable, // UNRELIABLE
        UnreliableSequenced, // UNRELIABLE_SEQUENCED
        Reliable, // RELIABLE
        ReliableOrdered, // RELIABLE_ORDERED
        ReliableSequenced // RELIABLE_SEQUENCED
    }
}
