using System.Reflection;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket.Metadata;

namespace FOMServer.Shared.Core.FOMPacket
{
    public static class PacketHelpers
    {
        /// <summary>
        /// A map for unwrapping data structs from the union by packet ID.
        /// </summary>
        private static readonly Dictionary<PacketIdentifier, FieldInfo> s_unionFieldsByID;

        /// <summary>
        /// A map for getting the packet ID associated with a data struct by type.
        /// </summary>
        private static readonly Dictionary<Type, PacketIdentifier> s_idByUnionType;

        /// <summary>
        /// Populates the reflection caches so that packet processing
        /// doesn't have to repeatedly use reflection at runtime.
        /// </summary>
        static PacketHelpers()
        {
            s_unionFieldsByID = [];
            s_idByUnionType = [];

            var unionFields = typeof(FOMDataUnion).GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var field in unionFields)
            {
                var type = field.FieldType;
                var idAttr = type.GetCustomAttribute<PacketIDAttribute>();
                if (idAttr == null)
                    continue;

                s_unionFieldsByID[idAttr.ID] = field;
                s_idByUnionType[type] = idAttr.ID;
            }
        }

        /// <summary>
        /// Wraps a data struct into a union and returns its associated packet ID.
        /// </summary>
        public static FOMDataUnion Wrap<TData>(TData data, out PacketIdentifier id) where TData : unmanaged
        {
            var type = typeof(TData);

            if (!s_idByUnionType.TryGetValue(type, out id))
                throw new ArgumentException($"Type {type.Name} is not mapped to any PacketID.");

            if (!s_unionFieldsByID.TryGetValue(id, out var field))
                throw new ArgumentException($"No union field found for packet ID {id}.");

            var union = new FOMDataUnion();
            field.SetValueDirect(__makeref(union), data);
            return union;
        }

        /// <summary>
        /// Unwraps a packet's data union to return the strongly-typed data struct.
        /// Validates that the packet ID matches the expected type.
        /// </summary>
        public static TData Unwrap<TData>(Packet packet) where TData : struct
        {
            var type = typeof(TData);

            if (!s_idByUnionType.TryGetValue(type, out var expectedID))
                throw new ArgumentException($"Type {type.Name} is not mapped to any PacketID.");

            if (packet.ID != expectedID)
                throw new ArgumentException($"Packet ID {packet.ID} does not match expected type {type.Name} (ID {expectedID}).");

            if (!s_unionFieldsByID.TryGetValue(packet.ID, out var field))
                throw new ArgumentException($"No union field found for packet ID {packet.ID}.");

            var value = field.GetValueDirect(__makeref(packet.Data));
            return (TData)value!;
        }
    }
}
