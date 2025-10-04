using FOMServer.Shared.Core.Enums;

namespace FOMServer.Shared.Metadata
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class PacketHandlerAttribute : Attribute
    {
        public PacketIdentifier ID { get; }

        public PacketHandlerAttribute(PacketIdentifier id)
        {
            ID = id;
        }
    }
}
