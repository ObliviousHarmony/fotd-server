using FOMServer.Master.Core.Networking;
using FOMServer.Shared.Core.PacketHandlers;
using FOMServer.Shared.Interop.FOMNetwork;
using FOMServer.Shared.Interop.FOMNetwork.Enums;
using FOMServer.Shared.Interop.FOMNetwork.Packets;
using FOMServer.Shared.Metadata;

namespace FOMServer.Master.Application.PacketHandlers
{
    [PacketHandler]
    internal class RegisterWorldPacketHandler : PacketHandlerBase<RegisterWorldPacket>
    {
        private readonly ILogger<RegisterWorldPacketHandler> _logger;
        private readonly IWorldServerRegistry _worldServerRegistry;

        public RegisterWorldPacketHandler(
            ILogger<RegisterWorldPacketHandler> logger,
            IWorldServerRegistry worldServerRegistry
        )
        {
            _logger = logger;
            _worldServerRegistry = worldServerRegistry;
        }

        public override void Handle(NetworkAddress sender, in RegisterWorldPacket p)
        {
            if (p.WorldIdCount <= 0)
            {
                throw new InvalidOperationException(
                    $"World server '{sender}' did not send any world ItemIds to register"
                );
            }

            var worldIds = new WorldId[p.WorldIdCount];
            for (var i = 0; i < p.WorldIdCount; i++)
            {
                worldIds[i] = p.WorldIds[i];
            }

            var registered = _worldServerRegistry.Register(worldIds, sender, p.PublicAddress);
            foreach (var worldId in registered)
            {
                _logger.LogInformation(
                    "World '{WorldId}' ({ServerAddress}) ready for clients at {PublicAddress}",
                    worldId,
                    sender,
                    p.PublicAddress
                );
            }
        }
    }
}
