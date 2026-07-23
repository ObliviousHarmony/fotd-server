using FOMServer.Shared.Interop.FOMNetwork.Enums;

namespace FOMServer.World.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when a lock acquisition on a player attribute times out,
    /// indicating a potential deadlock.
    /// </summary>
    internal class AttributeDeadlockException : Exception
    {
        public AttributeDeadlockException(AttributeType attribute)
            : base($"Potential deadlock acquiring lock on {attribute}")
        {
            Attribute = attribute;
        }

        public AttributeType Attribute { get; }
    }
}
