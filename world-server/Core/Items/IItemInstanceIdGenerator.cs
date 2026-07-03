namespace FOMServer.World.Core.Items
{
    /// <summary>
    /// Allocates unique, non-zero instance ids for item instances.
    /// </summary>
    /// <remarks>
    /// Ids are session-scoped for now: they are handed out from an in-memory
    /// counter and are not durable. Once DB-backed inventory persistence lands,
    /// durable ids will be assigned by the store instead.
    /// </remarks>
    internal interface IItemInstanceIdGenerator
    {
        /// <summary>
        /// Returns the next unique, non-zero instance id.
        /// </summary>
        uint Next();
    }
}
