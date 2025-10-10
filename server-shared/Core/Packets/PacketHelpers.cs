using System.Reflection;
using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Packets
{
    public static class PacketHelpers
    {
        /// <summary>
        /// A map for getting the packet ID associated with a data struct by type.
        /// </summary>
        private static readonly Dictionary<Type, PacketIdentifier> s_idByPacketType;

        /// <summary>
        /// A map for the sizes of each packet type by its identifier.
        /// </summary>
        private static readonly Dictionary<PacketIdentifier, int> s_packetSizes;

        /// <summary>
        /// Populates the reflection caches so that packet processing
        /// doesn't have to repeatedly use reflection at runtime.
        /// </summary>
        static PacketHelpers()
        {
            s_idByPacketType = [];
            s_packetSizes = [];

            var allTypes = typeof(PacketHelpers).Assembly.GetTypes();
            foreach (var type in allTypes)
            {
                // Check for your PacketIDAttribute
                var idAttr = type.GetCustomAttribute<PacketIDAttribute>();
                if (idAttr == null)
                    continue;

                s_idByPacketType[type] = idAttr.ID;
                s_packetSizes[idAttr.ID] = Marshal.SizeOf(type);
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
            if (!s_packetSizes.TryGetValue(id, out var size))
                throw new ArgumentException($"No size found for ID {id}");
            return size;
        }

        /// <summary>
        /// Returns the size of the given packet struct.
        /// </summary>
        public static int GetPacketSize<TPacket>() where TPacket : unmanaged
        {
            var type = typeof(TPacket);
            if (!s_idByPacketType.TryGetValue(type, out var id))
                throw new ArgumentException($"Type {type.Name} is not mapped to any PacketID");
            return GetPacketSize(id);
        }

        /// <summary>
        /// Returns the packet ID of the given packet type
        /// </summary>
        public static PacketIdentifier GetPacketTypeID<TPacket>() where TPacket : unmanaged
        {
            var type = typeof(TPacket);
            if (!s_idByPacketType.TryGetValue(type, out var expectedID))
                throw new ArgumentException($"Type {type.Name} is not mapped to any PacketID");
            return expectedID;
        }

        /// <summary>
        /// Checks to see if a given packet type matches what the ID expects.
        /// </summary>
        public static bool IsPacketOfType<TPacket>(PacketIdentifier id) where TPacket : unmanaged
        {
            var type = typeof(TPacket);
            if (!s_idByPacketType.TryGetValue(type, out var expectedID))
                throw new ArgumentException($"Type {type.Name} is not mapped to any PacketID");
            return id == expectedID;
        }
    }
}
