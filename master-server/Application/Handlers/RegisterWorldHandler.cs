using FOMServer.Master.Core.Networking;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Metadata;

namespace FOMServer.Master.Application.Handlers
{
    [PacketHandler]
    internal class RegisterWorldHandler : PacketHandlerBase<RegisterWorld>
    {
        private readonly ILogger<RegisterWorldHandler> _logger;
        private readonly IWorldServerRegistry _worldServerRegistry;

        public RegisterWorldHandler(ILogger<RegisterWorldHandler> logger, IWorldServerRegistry worldServerRegistry)
        {
            _logger = logger;
            _worldServerRegistry = worldServerRegistry;
        }

        public override void Handle(NetworkAddress sender, in RegisterWorld p)
        {
            if (p.WorldIdCount <= 0)
            {
                throw new InvalidOperationException($"World server '{sender}' did not send any world Ids to register");
            }

            var worldIds = new WorldId[p.WorldIdCount];
            for (var i = 0; i < p.WorldIdCount; i++)
            {
                worldIds[i] = p.WorldIds[i];
            }

            var registered = _worldServerRegistry.Register(worldIds, sender, p.PublicAddress);
            foreach (var worldId in registered)
            {
                _logger.LogInformation("World '{WorldId}' ({ServerAddress}) ready for clients at {PublicAddress}", worldId, sender, p.PublicAddress);
            }
        }
    }
}
