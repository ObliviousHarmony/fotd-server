using System.Reflection;
using System.Runtime.InteropServices;
using FOMServer.Shared.Core.FOMPacket;
using FOMServer.Shared.Core.FOMPacket.Metadata;

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
    }

}
