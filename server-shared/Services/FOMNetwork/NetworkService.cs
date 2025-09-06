using FOMServer.Shared.Enums;
using FOMServer.Shared.Models;
using FOMServer.Shared.Packets;
using System.Runtime.InteropServices;

namespace FOMServer.Shared.Services.FOMNetwork
{
	public partial class NetworkService : INetworkService
	{
		/// <inheritdoc />
		public void ValidateFOMPacket()
		{
			PacketStructure[] structures;
			try
			{
				structures = [
					new PacketStructure { ID = PacketIdentifier.ID_FOM_PACKET_ERROR, Size = Marshal.SizeOf<FOMPacketError>() },
				];
			} catch (Exception ex)
			{
				throw new Exception("Failed to calculate struct sizes for FOMPacket structures. Ensure all FOMPacket structs are blittable.", ex);
			}

			var ret = FOMNetwork_ValidatePacketStructs(structures, structures.Length);
			if (ret == -1)
				throw new Exception("The number of structs provided does not match the number expected by the network library.");
			else if (ret == -1)
				throw new Exception("The network library was asked to validate a struct that does not exist.");
			else if (ret == -3)
				throw new Exception("One or more of the provided structs does not match the expected size.");
		}

		[LibraryImport("FOMNetwork")]
		private static partial sbyte FOMNetwork_ValidatePacketStructs(PacketStructure[] structures, int count);
	}
}
