using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Utilities;
using FOMServer.Shared.Infrastructure.FOMNetwork;
using NetworkAddress = FOMServer.Shared.Core.Packets.Types.NetworkAddress;

namespace FOMServer.Shared.Services.FOMNetwork
{
    internal partial class NetworkService : INetworkService
    {
        public void ValidatePacketStructs()
        {
            // Ensure all of the API communication structs are blittable.
            AssertBlittable<NetworkAddress>();
            AssertBlittable<PacketStructure>();
            AssertBlittable<ReceivedPackets>();
            AssertBlittable<SendPacket>();

            // Make sure that the packet structs match what the network library expects.
            var structures = PacketHelpers.GetPacketStructures();
            var ret = FOMNetwork_ValidatePacketStructs(structures, structures.Length);
            if (ret == -1)
            {
                throw new InvalidOperationException(
                    "The number of structs provided does not match the number expected by the network library"
                );
            }
            else if (ret == -2)
            {
                throw new InvalidOperationException(
                    "The network library was asked to validate a struct that does not exist"
                );
            }
            else if (ret == -3)
            {
                throw new InvalidOperationException(
                    "One or more of the provided structs does not match the expected size"
                );
            }
        }

        /// <summary>
        /// This will throw a compile-time error if T is not a blittable struct.
        /// </summary>
        private static void AssertBlittable<T>()
            where T : struct { }

        [LibraryImport("FOMNetwork")]
        private static partial int FOMNetwork_ValidatePacketStructs(PacketStructure[] structures, int count);
    }
}
