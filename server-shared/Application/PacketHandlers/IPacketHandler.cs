using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Models;

namespace FOMServer.Shared.Application.PacketHandlers
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
