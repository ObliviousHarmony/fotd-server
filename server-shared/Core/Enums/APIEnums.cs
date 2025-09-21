namespace FOMServer.Shared.Core.Enums
{
	/// <summary>
	/// The priority for a packet to be sent with.
	/// </summary>
	public enum PacketPriority : byte
	{
		HIGH_PRIORITY = 1,
		MEDIUM_PRIORITY,
		LOW_PRIORITY
	}

	/// <summary>
	/// How reliable a packet should be when sent.
	/// </summary>
	public enum PacketReliability : byte
	{
		UNRELIABLE,
		UNRELIABLE_SEQUENCED,
		RELIABLE,
		RELIABLE_ORDERED,
		RELIABLE_SEQUENCED
	}
}
