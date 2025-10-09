using FOMServer.Shared.Core.Enums;
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
    public class PlayerEnteringWorldHandler : BasePacketHandler<PlayerEnteringWorld>
    {
        private readonly IPlayerService _playerService;
        private readonly IMasterPacketSender _packetSender;

        public PlayerEnteringWorldHandler(IMasterPacketSender packetSender, IPlayerService playerService)
        {
            _packetSender = packetSender;
            _playerService = playerService;
        }

        public override void Handle(NetworkAddress sender, in PlayerEnteringWorld p)
        {
            using var response = QueuePacket.Create<PlayerEnteringWorldReturn>();
            ref var rData = ref response.Data;

            

            rData.PlayerID = p.PlayerID;
            var player = _playerService.OnPlayerEnteringWorld(p.PlayerID, p.SelectedNodeID);
            if (player == null)
                rData.Status = PlayerEnteringWorldReturn.StatusCode.PLAYER_ENTERING_WORLD_RETURN_ALREADY_IN_WORLD;
            else
                rData.Status = PlayerEnteringWorldReturn.StatusCode.PLAYER_ENTERING_WORLD_RETURN_READY;

            Console.WriteLine($"Player {p.PlayerID} entering node {p.SelectedNodeID} - {rData.Status}");

            _packetSender.Send(response, PacketPriority.Medium, PacketReliability.ReliableOrdered);
        }
    }
}
