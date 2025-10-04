using System.Reflection;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket;
using FOMServer.Shared.Core.FOMPacket.Metadata;

namespace FOMServer.Shared.Extensions
{
    public static class FOMDataUnionExtensions
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
        static FOMDataUnionExtensions()
        {
            s_unionFieldsByID = [];
            s_idByUnionType = [];

            var unionFields = typeof(FOMDataUnion).GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var field in unionFields)
            {
                var type = field.FieldType;

                var idAttrs = type.GetCustomAttributes<PacketIDAttribute>().ToArray();

                // Record all IDs for this type so that it can be wrapped.
                // This lets us receive packets and populate the union
                // without knowing the exact type ahead of time.

                // Record a
                foreach (var attr in idAttrs)
                    s_unionFieldsByID[attr.ID] = field;

                // Only record 
                if (idAttrs.Length == 1)
                    s_idByUnionType[type] = idAttrs[0].ID;

            }
        }

        // --- Wrap: construct a union from a data struct ---
        public static FOMDataUnion Wrap<TData>(this TData data, out PacketIdentifier id) where TData : struct
        {
            if (!s_iDsByType.TryGetValue(typeof(TData), out var ids) || ids.Length == 0)
                throw new InvalidOperationException($"Type {typeof(TData).Name} does not have a PacketID attribute");

            id = ids[0]; // pick the first ID for sending

            var field = s_unionFieldsByID[id];
            var union = new FOMDataUnion();
            field.SetValueDirect(__makeref(union), data);

            return union;
        }

        // --- Unwrap: extract data from a union, validating packet ID ---
        public static TData Unwrap<TData>(this FOMDataUnion union, PacketIdentifier packetId) where TData : struct
        {
            if (!s_iDsByType.TryGetValue(typeof(TData), out var ids) || ids.Length == 0)
                throw new InvalidOperationException($"Type {typeof(TData).Name} does not have a PacketID attribute");

            if (!ids.Contains(packetId))
                throw new InvalidOperationException($"Packet ID {packetId} does not match type {typeof(TData).Name}");

            if (!s_unionFieldsByID.TryGetValue(packetId, out var field))
                throw new InvalidOperationException($"No field in FOMDataUnion for packet ID {packetId}");

            var obj = field.GetValueDirect(__makeref(union));
            if (obj is not TData typed)
                throw new InvalidOperationException($"Union field for packet ID {packetId} is not of type {typeof(TData).Name}");

            return typed;
        }


        private static IReadOnlyList<PacketIdentifier> GetPacketIDs<TData>() where TData : struct
        {
            if (!s_iDsByType.TryGetValue(typeof(TData), out var ids))
                throw new InvalidOperationException($"Type {typeof(TData).Name} does not have a PacketID attribute");

            return ids;
        }
    }
}
