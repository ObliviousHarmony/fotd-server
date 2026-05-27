using FOMServer.Shared.Core.Enums;

namespace FOMServer.Shared.Metadata
{
    [AttributeUsage(AttributeTargets.Struct, Inherited = false)]
    internal sealed class PacketIdAttribute : Attribute
    {
        public PacketIdAttribute(PacketIdentifier id)
        {
            Id = id;
        }

        public PacketIdentifier Id { get; }
    }
}
