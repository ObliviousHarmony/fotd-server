using FOMServer.Shared.Core.Enums;

namespace FOMServer.Shared.Metadata
{
    [AttributeUsage(AttributeTargets.Struct, Inherited = false)]
    internal sealed class PacketIDAttribute : Attribute
    {
        public PacketIDAttribute(PacketIdentifier id)
        {
            ID = id;
        }

        public PacketIdentifier ID { get; }
    }
}
