using System.Reflection;
using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Infrastructure.FOMNetwork;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Packets
{
    public static class PacketHelpers
    {
        /// <summary>
        /// The size of the largest registered packet.
        /// </summary>
        public static readonly int MaxPacketSize;

        /// <summary>
        /// A map for getting the packet ID associated with a data struct by type.
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
                // Check for your PacketIDAttribute
                var idAttr = type.GetCustomAttribute<PacketIDAttribute>();
                if (idAttr is null)
                {
                    continue;
                }

                var size = Marshal.SizeOf(type);
                if (size > MaxPacketSize)
                {
                    MaxPacketSize = size;
                }

                s_idByPacketType[type] = idAttr.ID;
                s_packetSizes[idAttr.ID] = size;
            }
        }

        /// <summary>
        /// Returns all of the structures used in the packet union along with their sizes.
        /// </summary>
        public static PacketStructure[] GetPacketStructures()
        {
            return [.. s_packetSizes.Select(kv => new PacketStructure
            {
                ID = kv.Key,
                Size = kv.Value
            })];
        }

        /// <summary>
        /// Returns the size of the struct associated with the given packet ID.
        /// </summary>
        public static int GetPacketSize(PacketIdentifier id)
        {
            return !s_packetSizes.TryGetValue(id, out var size) ? throw new ArgumentException($"No size found for ID '{id}'", nameof(id)) : size;
        }

        /// <summary>
        /// Returns the size of the given packet struct.
        /// </summary>
        public static int GetPacketSize<TPacket>() where TPacket : unmanaged
        {
            var type = typeof(TPacket);
            return !s_idByPacketType.TryGetValue(type, out var id)
                ? throw new InvalidOperationException($"Type {type.Name} is not mapped to any PacketID")
                : GetPacketSize(id);
        }

        /// <summary>
        /// Returns the packet ID of the given packet type
        /// </summary>
        public static PacketIdentifier GetPacketTypeID<TPacket>() where TPacket : unmanaged
        {
            var type = typeof(TPacket);
            return !s_idByPacketType.TryGetValue(type, out var expectedID)
                ? throw new InvalidOperationException($"Type {type.Name} is not mapped to any PacketID")
                : expectedID;
        }

        /// <summary>
        /// Checks to see if a given packet type matches what the ID expects.
        /// </summary>
        public static bool IsPacketOfType<TPacket>(PacketIdentifier id) where TPacket : unmanaged
        {
            var type = typeof(TPacket);
            return !s_idByPacketType.TryGetValue(type, out var expectedID)
                ? throw new InvalidOperationException($"Type {type.Name} is not mapped to any PacketID")
                : id == expectedID;
        }
    }
}
