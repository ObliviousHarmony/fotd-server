using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Buffers;
using FOMServer.Shared.Infrastructure.FOMNetwork;
using FOMServer.Shared.Interop.FOMNetwork.Enums;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Utilities
{
    public static class PacketHelpers
    {
        /// <summary>
        /// The size of the largest registered packet.
        /// </summary>
        public static readonly int MaxPacketSize;

        /// <summary>
        /// A map for getting the packet Id associated with a data struct by type.
        /// </summary>
        private static readonly Dictionary<Type, PacketIdentifier> s_idByPacketType = [];

        /// <summary>
        /// A map for the sizes of each packet type by its identifier.
        /// </summary>
        private static readonly Dictionary<PacketIdentifier, int> s_packetSizes = [];

        /// <summary>
        /// Populates the reflection caches so that packet processing
        /// doesn't have to repeatedly use reflection at runtime.
        /// </summary>
        static PacketHelpers()
        {
            MaxPacketSize = 0;
            var allTypes = typeof(PacketHelpers).Assembly.GetTypes();
            foreach (var type in allTypes)
            {
                // Check for your PacketIdAttribute
                var idAttr = type.GetCustomAttribute<PacketIdAttribute>();
                if (idAttr is null)
                {
                    continue;
                }

                var size = Marshal.SizeOf(type);
                if (size > MaxPacketSize)
                {
                    MaxPacketSize = size;
                }

                s_idByPacketType[type] = idAttr.Id;
                s_packetSizes[idAttr.Id] = size;
            }

            // The send path rents packet buffers from PinnedArrayPool, whose largest
            // bucket is MaximumBufferLength. A larger packet still works but falls back
            // to uncached pinned allocations, so flag it loudly during development.
            Debug.Assert(
                MaxPacketSize <= PinnedArrayPool.MaximumBufferLength,
                $"Largest packet ({MaxPacketSize} bytes) exceeds the pinned pool's largest bucket "
                    + $"({PinnedArrayPool.MaximumBufferLength} bytes)."
            );
        }

        /// <summary>
        /// Returns all of the structures used in the packet union along with their sizes.
        /// </summary>
        public static PacketStructure[] GetPacketStructures()
        {
            return [.. s_packetSizes.Select(kv => new PacketStructure { Id = kv.Key, Size = kv.Value })];
        }

        /// <summary>
        /// Returns the size of the struct associated with the given packet Id.
        /// </summary>
        public static int GetPacketSize(PacketIdentifier id)
        {
            return !s_packetSizes.TryGetValue(id, out var size)
                ? throw new ArgumentException($"No size found for Id '{id}'", nameof(id))
                : size;
        }

        /// <summary>
        /// Returns the size of the given packet struct.
        /// </summary>
        public static int GetPacketSize<TPacket>()
            where TPacket : unmanaged
        {
            var type = typeof(TPacket);
            return !s_idByPacketType.TryGetValue(type, out var id)
                ? throw new InvalidOperationException($"Type {type.Name} is not mapped to any PacketId")
                : GetPacketSize(id);
        }

        /// <summary>
        /// Returns the packet Id of the given packet type
        /// </summary>
        public static PacketIdentifier GetPacketTypeId<TPacket>()
            where TPacket : unmanaged
        {
            var type = typeof(TPacket);
            return !s_idByPacketType.TryGetValue(type, out var expectedId)
                ? throw new InvalidOperationException($"Type {type.Name} is not mapped to any PacketId")
                : expectedId;
        }

        /// <summary>
        /// Checks to see if a given packet type matches what the Id expects.
        /// </summary>
        public static bool IsPacketOfType<TPacket>(PacketIdentifier id)
            where TPacket : unmanaged
        {
            var type = typeof(TPacket);
            return !s_idByPacketType.TryGetValue(type, out var expectedId)
                ? throw new InvalidOperationException($"Type {type.Name} is not mapped to any PacketId")
                : id == expectedId;
        }
    }
}
