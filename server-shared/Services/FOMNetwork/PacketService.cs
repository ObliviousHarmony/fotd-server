using FOMServer.Shared.Models;
using System.Buffers;
using System.Runtime.InteropServices;

namespace FOMServer.Shared.Services.FOMNetwork
{
	public partial class PacketService : IPacketService
	{
		/// <inheritdoc />
		public ReceivedPackets Receive(IntPtr peer) => FOMNetwork_ReceivePackets(peer);

		/// <inheritdoc />
		public void Process(IntPtr peer, ref ReceivedPackets received, Span<FOMPacket> packetBuffer)
		{
			if (packetBuffer.Length < received.Count)
				throw new ArgumentException("Buffer too small.");

			unsafe
			{
				fixed (FOMPacket* ptr = &MemoryMarshal.GetReference(packetBuffer))
				{
					FOMNetwork_ProcessPackets(peer, received, ptr, received.Count);
				}
			}
		}

		/// <inheritdoc />
		public void Send(IntPtr peer, SendPacket[] packets) => FOMNetwork_Send(peer, packets, packets.Length);

		[LibraryImport("FOMNetwork")]
		private static partial ReceivedPackets FOMNetwork_ReceivePackets(IntPtr peer);

		[LibraryImport("FOMNetwork")]
		private static unsafe partial sbyte FOMNetwork_ProcessPackets(IntPtr peer, ReceivedPackets received, FOMPacket* packetBuffer, int packetBufferLen);

		[LibraryImport("FOMNetwork")]
		private static partial void FOMNetwork_Send(IntPtr peer, SendPacket[] packets, int count);
	}
}
