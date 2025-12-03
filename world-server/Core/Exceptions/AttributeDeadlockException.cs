using FOMServer.Shared.Core.Enums;

namespace FOMServer.World.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when a lock acquisition on a player attribute times out,
    /// indicating a potential deadlock.
    /// </summary>
    public class AttributeDeadlockException : Exception
    {
        public AttributeDeadlockException(PlayerAttribute attribute)
            : base($"Potential deadlock acquiring lock on {attribute}")
        {
            Attribute = attribute;
        }

        public PlayerAttribute Attribute { get; }
    }
}
