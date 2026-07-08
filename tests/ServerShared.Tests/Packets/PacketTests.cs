using System.Reflection;
using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Tests.Packets
{
    public class PacketTests
    {
        [Fact]
        public void Packet_PacketData_ShouldHaveLayoutAttribute()
        {
            // Every packet struct must explicitly define the memory layout to
            // ensure that it matches the C++ layout exactly.
            var packetTypes = typeof(IPacketHandler).Assembly.GetTypes()
                .Where(t => t.GetCustomAttribute<PacketIdAttribute>() is not null)
                .ToList();

            foreach (var type in packetTypes)
            {
                var layout = type.StructLayoutAttribute;
                if (layout is null || layout.Value != LayoutKind.Sequential || layout.Pack != 1)
                {
                    Assert.Fail($"{type.Name} must be declared with [StructLayout(LayoutKind.Sequential, Pack = 1)]");
                }
            }
        }

        [Fact]
        public void Packet_PacketHandler_ShouldBeDefinedCorrectly()
        {
            var assemblies = new[] {
                typeof(IPacketHandler).Assembly,
                typeof(Master.Application.Server).Assembly,
                typeof(World.Application.Server).Assembly,
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
                    attr is not null,
                    $"Packet handler '{type.FullName}' is missing [PacketHandler] attribute"
                );
            }
        }

        [Fact]
        public void Packet_PacketHandlerAttribute_ShouldOnlyBeOnHandlers()
        {
            var assemblies = new[] {
                typeof(IPacketHandler).Assembly,
                typeof(Master.Application.Server).Assembly,
                typeof(World.Application.Server).Assembly,
            };

            var attributedTypes = assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => t.GetCustomAttribute<PacketHandlerAttribute>() is not null)
                .ToArray();

            Assert.NotEmpty(attributedTypes); // sanity check: make sure we found some

            var invalidType = attributedTypes
                .Where(t => !IsAssignableToGenericType(t, typeof(PacketHandlerBase<>)))
                .FirstOrDefault();

            Assert.True(
                invalidType is null,
                $"Type '{invalidType?.FullName}' has [PacketHandler] but does not inherit from BasePacketHandler<T>"
            );
        }

        /// <summary>
        /// Walks up the inheritance chain to determine if 'toCheck' is a subclass of the generic type 'genericBase'.
        /// </summary>
        private static bool IsAssignableToGenericType(Type givenType, Type genericType)
        {
            if (givenType is null || genericType is null)
            {
                return false;
            }

            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
            {
                return true;
            }

            foreach (var it in givenType.GetInterfaces())
            {
                if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
                {
                    return true;
                }
            }

            var baseType = givenType.BaseType;
            return baseType is not null && IsAssignableToGenericType(baseType, genericType);
        }
    }
}
