using FOMServer.Shared.Enums;
using FOMServer.Shared.Models;
using FOMServer.Shared.Packets;
using System;

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
				case PacketIdentifier.ID_FOM_PACKET_READ_ERROR when typeof(TPacket) == typeof(ReadPacketError):
					return (TPacket)(object)packet.Data.readError;
				case PacketIdentifier.ID_LOGIN_REQUEST when typeof(TPacket) == typeof(LoginRequest):
					return (TPacket)(object)packet.Data.loginRequest;

				default:
					throw new InvalidOperationException(
						$"Packet ID {packet.ID} does not match the requested type {typeof(TPacket).Name}");
			}
		}
	}
}
