using FOMServer.Shared.Core.Enums;

namespace FOMServer.Shared.Core.FOMPacket.Metadata
{
    [AttributeUsage(AttributeTargets.Struct, Inherited = false)]
    internal sealed class PacketIDAttribute : Attribute
    {
        public PacketIdentifier ID { get; }

        public PacketIDAttribute(PacketIdentifier id)
        {
            ID = id;
        }
    }
}
