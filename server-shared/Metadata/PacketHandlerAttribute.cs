using FOMServer.Shared.Core.Enums;

namespace FOMServer.Shared.Metadata
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class PacketHandlerAttribute : Attribute
    {
    }
}
