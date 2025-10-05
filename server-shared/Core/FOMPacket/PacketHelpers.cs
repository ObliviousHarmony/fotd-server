using System.Reflection;
using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.FOMPacket
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










            s_unionFieldsByID = [];
            var unionFields = typeof(FOMDataUnion).GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var field in unionFields)
            {
                var type = field.FieldType;
                var idAttr = type.GetCustomAttribute<PacketIDAttribute>();
                if (idAttr == null)
                    continue;

                s_unionFieldsByID[idAttr.ID] = field;
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

        public static bool IsPacketOfType<TPacket>(PacketIdentifier id) where TPacket : unmanaged
        {
            var type = typeof(TPacket);
            if (!s_idByPacketType.TryGetValue(type, out var expectedID))
                throw new ArgumentException($"Type {type.Name} is not mapped to any PacketID");
            return id == expectedID;
        }












        /// <summary>
        /// A map for unwrapping data structs from the union by packet ID.
        /// </summary>
        private static readonly Dictionary<PacketIdentifier, FieldInfo> s_unionFieldsByID;




        /// <summary>
        /// Wraps a data struct into a union and returns its associated packet ID.
        /// </summary>
        public static FOMDataUnion Wrap<TData>(TData data, out PacketIdentifier id) where TData : unmanaged
        {
            var type = typeof(TData);

            if (!s_idByPacketType.TryGetValue(type, out id))
                throw new ArgumentException($"Type {type.Name} is not mapped to any PacketID");

            if (!s_unionFieldsByID.TryGetValue(id, out var field))
                throw new ArgumentException($"No union field found for packet ID {id}");

            var union = new FOMDataUnion();
            field.SetValueDirect(__makeref(union), data);
            return union;
        }

        /// <summary>
        /// Unwraps a packet's data union to return the strongly-typed data struct.
        /// Validates that the packet ID matches the expected type.
        /// </summary>
        public static TData Unwrap<TData>(Packet packet, out PacketIdentifier id) where TData : struct
        {
            var type = typeof(TData);

            if (!s_idByPacketType.TryGetValue(type, out var expectedID))
                throw new ArgumentException($"Type {type.Name} is not mapped to any PacketID");

            if (packet.ID != expectedID)
                throw new ArgumentException($"Packet ID {packet.ID} does not match expected type {type.Name} (ID {expectedID})");

            if (!s_unionFieldsByID.TryGetValue(packet.ID, out var field))
                throw new ArgumentException($"No union field found for packet ID {packet.ID}");

            id = expectedID;
            var value = field.GetValueDirect(__makeref(packet.Data));
            return (TData)value!;
        }
    }
}
