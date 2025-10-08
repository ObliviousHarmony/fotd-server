using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Packets.Data;
using FOMServer.Shared.Core.Packets.Models;
using FOMServer.Shared.Metadata;
using FOMServer.World.Core.Networking;
using FOMServer.World.Core.Players;

namespace FOMServer.World.Application.Handlers
{
    [PacketHandler]
    public class RegisterClientHandler : BasePacketHandler<RegisterClient>
    {
        private readonly IPlayerService _playerService;
        private readonly IClientPacketSender _packetSender;

        public RegisterClientHandler(IClientPacketSender packetSender, IPlayerService playerService)
        {
            _packetSender = packetSender;
            _playerService = playerService;
        }

        public override void Handle(NetworkAddress sender, in RegisterClient p)
        {
            var player = _playerService.OnPlayerEnteredWorld(p.PlayerID, sender);
            if (player == null)
                throw new InvalidOperationException($"Player {p.PlayerID} not found");

            using var response = QueuePacket.Create<RegisterClientReturn>();
            ref var rData = ref response.Data;

            rData.WorldID = p.WorldID;
            rData.PlayerID = p.PlayerID;
            rData.Status = RegisterClientReturn.StatusCode.REGISTER_CLIENT_RETURN_SUCCESS;
            rData.Attributes[PlayerAttribute.Health] = 1000;
            rData.Attributes[PlayerAttribute.Stamina] = 1000;
            rData.Attributes[PlayerAttribute.Aura] = 1000;
            rData.Attributes[PlayerAttribute.Bioenergy] = 1000;
            rData.Attributes[PlayerAttribute.UC] = 1234;
            rData.Attributes[PlayerAttribute.Coins] = 5678;
            rData.Name = "Oblivious Test";
            rData.SelectedNode = player.SelectedNodeID;

            _packetSender.Send(response, sender, PacketPriority.Medium, PacketReliability.ReliableOrdered);
        }
    }
}
