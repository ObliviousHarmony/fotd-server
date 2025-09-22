using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Models;
using FOMServer.Shared.Core.Models.FOMData;
using FOMServer.Shared.Infrastructure.FOMNetwork;
using System.Runtime.InteropServices;

namespace FOMServer.Shared.Services.FOMNetwork
{
	public partial class NetworkService : INetworkService
	{
		public void ValidateFOMPacket()
		{
			// Ensure all of the API communication structs are blittable.
			if (!IsBlittable<NetworkAddress>())
				throw new Exception("The NetworkAddress struct is not blittable.");
			if (!IsBlittable<PacketStructure>())
				throw new Exception("The PacketStructure struct is not blittable.");
			if (!IsBlittable<ReceivedPackets>())
				throw new Exception("The ReceivedPackets struct is not blittable.");
			if (!IsBlittable<SendPacket>())
				throw new Exception("The SendPacket struct is not blittable.");

			// Ensure all packet data structs are blittable.
			PacketStructure[] structures;
			structures = [
				new PacketStructure { ID = PacketIdentifier.ID_FOM_PACKET_READ_ERROR, Size = Marshal.SizeOf<ReadPacketError>() },
				new PacketStructure { ID = PacketIdentifier.ID_LOGIN_REQUEST, Size = Marshal.SizeOf<LoginRequest>() },
				new PacketStructure { ID = PacketIdentifier.ID_LOGIN_REQUEST_RETURN, Size = Marshal.SizeOf<LoginRequestReturn>() }
			];
			foreach (PacketStructure s in structures)
			{
				if (!IsBlittable<ReadPacketError>())
					throw new Exception($"The data struct for packet ID {s.ID} is not blittable.");
			}

			int ret = FOMNetwork_ValidatePacketStructs(structures, structures.Length);
			if (ret == -1)
				throw new Exception("The number of structs provided does not match the number expected by the network library.");
			else if (ret == -1)
				throw new Exception("The network library was asked to validate a struct that does not exist.");
			else if (ret == -3)
				throw new Exception("One or more of the provided structs does not match the expected size.");
		}

		private static bool IsBlittable<T>() where T : struct
		{
			try
			{
				// An exception will be thrown if the type is not blittable.
				var handle = GCHandle.Alloc(default(T), GCHandleType.Pinned);
				handle.Free();
				return true;
			}
			catch (ArgumentException)
			{
				return false;
			}
		}

		[LibraryImport("FOMNetwork")]
		private static partial int FOMNetwork_ValidatePacketStructs(PacketStructure[] structures, int count);
	}
}
