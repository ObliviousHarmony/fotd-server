using FOMServer.Master.Core.Networking;
using FOMServer.Master.Core.Players;
using FOMServer.Shared.Core.PacketHandlers;
using FOMServer.Shared.Interop.FOMNetwork;
using FOMServer.Shared.Interop.FOMNetwork.Packets.RakNet;
using FOMServer.Shared.Metadata;

namespace FOMServer.Master.Application.PacketHandlers
{
    [PacketHandler]
    internal class DisconnectionPacketHandler : PacketHandlerBase<DisconnectionNotificationPacket>
    {
        private readonly IWorldServerRegistry _worldServerRegistry;
        private readonly IClientRegistry _clientRegistry;
        private readonly IPlayerRegistry _playerRegistry;
        private readonly ILogger<DisconnectionPacketHandler> _logger;

        public DisconnectionPacketHandler(
            IWorldServerRegistry worldServerRegistry,
            IClientRegistry clientRegistry,
            IPlayerRegistry playerRegistry,
            ILogger<DisconnectionPacketHandler> logger
        )
        {
            _worldServerRegistry = worldServerRegistry;
            _clientRegistry = clientRegistry;
            _playerRegistry = playerRegistry;
            _logger = logger;
        }

        public override void Handle(NetworkAddress sender, in DisconnectionNotificationPacket p)
        {
            if (TryWorldServerUnregister(sender))
            {
                return;
            }

            TryClientUnregister(sender);
        }

        private bool TryWorldServerUnregister(NetworkAddress sender)
        {
            var unregistered = _worldServerRegistry.Unregister(sender);
            if (unregistered.Length == 0)
            {
                return false;
            }

            foreach (var worldId in unregistered)
            {
                _logger.LogInformation("World '{WorldId}' disconnected", worldId);
            }

            return true;
        }

        private void TryClientUnregister(NetworkAddress sender)
        {
            var session = _clientRegistry.Get(sender);
            if (session is null)
            {
                return;
            }

            if (session.Player is not null)
            {
                _playerRegistry.Logout(session.Player);
            }

            _clientRegistry.Unregister(session);
        }
    }
}
