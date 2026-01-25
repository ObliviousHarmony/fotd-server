using FOMServer.Master.Core.Networking;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Metadata;

namespace FOMServer.Master.Application.Handlers
{
    [PacketHandler]
    public class RegisterWorldPacketHandler : PacketHandlerBase<RegisterWorld>
    {
        private readonly ILogger<RegisterWorldPacketHandler> _logger;
        private readonly IWorldServerRegistry _worldServerRegistry;

        public RegisterWorldPacketHandler(ILogger<RegisterWorldPacketHandler> logger, IWorldServerRegistry worldServerRegistry)
        {
            _logger = logger;
            _worldServerRegistry = worldServerRegistry;
        }

        public override void Handle(NetworkAddress sender, in RegisterWorld p)
        {
            if (p.WorldIDCount <= 0)
                throw new InvalidOperationException($"World server '{sender}' did not send any world IDs to register");

            var worldIDs = new WorldID[p.WorldIDCount];
            for (int i = 0; i < p.WorldIDCount; i++)
                worldIDs[i] = p.WorldIDs[i];

            var registered = _worldServerRegistry.Register(worldIDs, sender, p.PublicAddress);
            foreach (var worldID in registered)
                _logger.LogInformation("World '{WorldID}' ({ServerAddress}) ready for clients at {PublicAddress}", worldID, sender, p.PublicAddress);
        }
    }
}
