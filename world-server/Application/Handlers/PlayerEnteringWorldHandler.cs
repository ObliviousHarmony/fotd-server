using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Packets.Data;
using FOMServer.Shared.Metadata;
using FOMServer.World.Core.Networking;
using FOMServer.World.Core.Players;

namespace FOMServer.World.Application.Handlers
{
    [PacketHandler]
    public class PlayerEnteringWorldHandler : PacketHandlerBase<PlayerEnteringWorld>
    {
        private readonly IMasterPacketSender _packetSender;
        private readonly IPlayerRegistry _playerRegistry;
        private readonly ILoginService _loginService;

        public PlayerEnteringWorldHandler(
            IMasterPacketSender packetSender,
            IPlayerRegistry playerRegistry,
            ILoginService loginService)
        {
            _packetSender = packetSender;
            _playerRegistry = playerRegistry;
            _loginService = loginService;
        }

        public override void Handle(NetworkAddress sender, in PlayerEnteringWorld p)
        {
            using var response = new PacketWriter<PlayerEnteringWorldReturn>();
            ref var rData = ref response.Data;

            rData.PlayerID = p.PlayerID;

            // Check if player is already in this world
            if (_playerRegistry.Get(p.PlayerID) != null)
            {
                rData.Status = PlayerEnteringWorldReturn.StatusCode.PLAYER_ENTERING_WORLD_RETURN_ALREADY_IN_WORLD;
            }
            else
            {
                _loginService.Prepare(p.PlayerID, p.SelectedNodeID);
                rData.Status = PlayerEnteringWorldReturn.StatusCode.PLAYER_ENTERING_WORLD_RETURN_READY;
            }

            _packetSender.Send(response.Build());
        }
    }
}
