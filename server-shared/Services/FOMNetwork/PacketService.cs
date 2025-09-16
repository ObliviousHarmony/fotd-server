using FOMServer.Shared.Models;
using System.Runtime.InteropServices;

namespace FOMServer.Shared.Services.FOMNetwork
{
	public partial class PacketService : IPacketService
	{
		// A buffer to avoid allocating memory for holding the packets that have been received.
		// This is shared across calls to Receive, so it must be cleared before each use.

		/// <summary>
		/// A static buffer for receiving packets to avoid allocations.
		/// </summary>
		/// <remarks>
		/// In order for this buffer to be safe to use, the returned span MUST not be used after
		/// the next call to Receive, as it will be overwritten.
		/// </remarks>
		private static readonly FOMPacket[] ReceiveBuffer = new FOMPacket[IPacketService.MaxBufferedPackets];

		/// <inheritdoc />
		public Span<FOMPacket> Receive(IntPtr peer)
		{
			ReceivedPackets received = FOMNetwork_ReceivePackets(peer);
			if (received.Count == 0)
				return Span<FOMPacket>.Empty;

			unsafe
			{
				fixed (FOMPacket* bufferPtr = ReceiveBuffer)
				{
					if (FOMNetwork_ProcessPackets(peer, received, bufferPtr, received.Count) != 0)
						return Span<FOMPacket>.Empty;
				}
			}

			return ReceiveBuffer.AsSpan(0, received.Count);
		}

		/// <inheritdoc />
		public void Send(IntPtr peer, Span<SendPacket> packets)
		{
			if (packets.IsEmpty)
				return;

			unsafe
			{
				fixed (SendPacket* ptr = packets)
				{
					FOMNetwork_Send(peer, ptr, packets.Length);
				}
			}
		}

		[LibraryImport("FOMNetwork")]
		private static partial ReceivedPackets FOMNetwork_ReceivePackets(IntPtr peer);

		[LibraryImport("FOMNetwork")]
		private static unsafe partial int FOMNetwork_ProcessPackets(IntPtr peer, ReceivedPackets received, FOMPacket* packetBuffer, int packetBufferLen);

		[LibraryImport("FOMNetwork")]
		private static unsafe partial int FOMNetwork_Send(IntPtr peer, SendPacket* packets, int count);
	}
}
