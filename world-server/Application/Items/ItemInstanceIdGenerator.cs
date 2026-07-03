using FOMServer.World.Core.Items;

namespace FOMServer.World.Application.Items
{
    internal sealed class ItemInstanceIdGenerator : IItemInstanceIdGenerator
    {
        // Seed above the historical placeholder instance id (1001) so freshly
        // granted items never collide with legacy hardcoded world-entry items.
        private long _next = 100_000;

        public uint Next()
        {
            return (uint)Interlocked.Increment(ref _next);
        }
    }
}
