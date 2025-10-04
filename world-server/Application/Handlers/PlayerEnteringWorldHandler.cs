using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket;
using FOMServer.Shared.Core.FOMPacket.Data;
using FOMServer.Shared.Core.Handlers;
using FOMServer.World.Core.Networking;
using FOMServer.World.Core.Players;

namespace FOMServer.World.Application.Handlers
{
    public class PlayerEnteringWorldHandler : PacketHandler<PlayerEnteringWorld>
    {
        public override PacketIdentifier PacketID => PacketIdentifier.ID_PLAYER_ENTERING_WORLD;

        private readonly IPlayerService _playerService;
        private readonly IMasterPacketSender _packetSender;

        public PlayerEnteringWorldHandler(IMasterPacketSender packetSender, IPlayerService playerService)
        {
            _packetSender = packetSender;
            _playerService = playerService;
        }

        public override void Handle(NetworkAddress sender, in PlayerEnteringWorld data)
        {
            var response = new PlayerEnteringWorldReturn
            {
                PlayerID = data.PlayerID,
            };

            var player = _playerService.OnPlayerEnteringWorld(data.PlayerID, data.SelectedNodeID);
            if (player == null)
                response.Status = PlayerEnteringWorldReturn.StatusCode.PLAYER_ENTERING_WORLD_RETURN_ALREADY_IN_WORLD;
            else
                response.Status = PlayerEnteringWorldReturn.StatusCode.PLAYER_ENTERING_WORLD_RETURN_READY;

            _packetSender.Send(response, PacketPriority.MEDIUM_PRIORITY, PacketReliability.RELIABLE_ORDERED);
        }
    }
}
