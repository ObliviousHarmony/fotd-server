using FOMServer.Shared.Enums;
using FOMServer.Shared.Models;

namespace FOMServer.Shared.Services.Packets
{
	/// <summary>
	/// An interface describing a handler for incoming packets.
	/// </summary>
	public interface IPacketHandler
	{
		PacketIdentifier PacketID { get; }

		void Handle(in FOMPacket packet);
	}
}
