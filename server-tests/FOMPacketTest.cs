using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using FOMServer.Shared.Core.FOMPacket;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Metadata;

namespace FOMServer.Tests
{
    public class FOMPacketTest
    {
        [Fact]
        public void FOMPacket_DataUnionFields_ShouldBeDefinedCorrectly()
        {
            // The FOMDataUnion struct is designed to replicate a C++ union, where all fields
            // overlap in memory. To achieve this in C#, each field must be marked with
            // [FieldOffset(0)] to ensure they all start at the same memory location.
            var unionFields = typeof(FOMDataUnion).GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var field in unionFields)
            {
                var idAttr = field.FieldType.GetCustomAttribute<PacketIDAttribute>();
                if (idAttr == null)
                    Assert.Fail($"Field '{field.Name}' type is missing [PacketID]");

                var offsetAttr = field.GetCustomAttribute<FieldOffsetAttribute>();
                if (offsetAttr == null || offsetAttr.Value != 0)
                    Assert.Fail($"Field '{field.Name}' is missing [FieldOffset(0)]");
            }
        }

        [Fact]
        public void FOMPacket_PacketData_ShouldBeInDataUnion()
        {
            // Ensures that all of the packet structs with [PacketID] are represented inside of the FOMDataUnion struct.
            var packetTypes = typeof(FOMDataUnion).Assembly.GetTypes()
                .Where(t => t.GetCustomAttribute<PacketIDAttribute>() != null)
                .ToList();

            var unionFields = typeof(FOMDataUnion).GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var type in packetTypes)
            {
                var found = unionFields.Any(f => f.FieldType == type);
                if (!found)
                    Assert.Fail($"{type.Name} has [PacketID] but is not represented in FOMDataUnion");
            }
        }

        [Fact]
        public void FOMPacket_PacketData_ShouldHaveLayoutAttribute()
        {
            // Every packet struct must explicitly define the memory layout to
            // ensure that it matches the C++ layout exactly.
            var packetTypes = typeof(FOMDataUnion).Assembly.GetTypes()
                .Where(t => t.GetCustomAttribute<PacketIDAttribute>() != null)
                .ToList();

            foreach (var type in packetTypes)
            {
                var layout = type.StructLayoutAttribute;
                if (layout == null || layout.Value != LayoutKind.Sequential || layout.Pack != 1)
                    Assert.Fail($"{type.Name} must be declared with [StructLayout(LayoutKind.Sequential, Pack = 1)]");
            }
        }

        [Fact]
        public void FOMPacket_PacketHandler_ShouldBeDefinedCorrectly()
        {
            var assemblies = new[] {
                typeof(IPacketHandler).Assembly,
                typeof(FOMServer.Master.Application.Server).Assembly,
                typeof(FOMServer.World.Application.Server).Assembly,
            };

            var handlerInterface = typeof(IPacketHandler);

            var handlerTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => handlerInterface.IsAssignableFrom(t) && !t.IsAbstract)
            .ToArray();

            Assert.NotEmpty(handlerTypes);

            foreach (var type in handlerTypes)
            {
                var attr = type.GetCustomAttribute<PacketHandlerAttribute>(inherit: false);

                Assert.True(
                    attr != null,
                    $"Packet handler '{type.FullName}' is missing [PacketHandler] attribute"
                );
            }
        }

        [Fact]
        public void FOMPacket_PacketHandlerAttribute_ShouldOnlyBeOnHandlers()
        {
            var assemblies = new[] {
                typeof(IPacketHandler).Assembly,
                typeof(FOMServer.Master.Application.Server).Assembly,
                typeof(FOMServer.World.Application.Server).Assembly,
            };

            var attributedTypes = assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => t.GetCustomAttribute<PacketHandlerAttribute>() != null)
                .ToArray();

            Assert.NotEmpty(attributedTypes); // sanity check: make sure we found some

            var invalidType = attributedTypes
                .Where(t => !IsAssignableToGenericType(t, typeof(BasePacketHandler<>)))
                .FirstOrDefault();

            Assert.True(
                invalidType == null,
                $"Type '{invalidType?.FullName}' has [PacketHandler] but does not inherit from BasePacketHandler<T>"
            );
        }

        /// <summary>
        /// Walks up the inheritance chain to determine if 'toCheck' is a subclass of the generic type 'genericBase'.
        /// </summary>
        private static bool IsAssignableToGenericType(Type givenType, Type genericType)
        {
            if (givenType == null || genericType == null)
                return false;

            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
                return true;

            foreach (var it in givenType.GetInterfaces())
            {
                if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
                    return true;
            }

            var baseType = givenType.BaseType;
            return baseType != null && IsAssignableToGenericType(baseType, genericType);
        }
    }
}
