using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Metadata;
using FOMServer.World.Core.Networking;
using FOMServer.World.Core.Players;

namespace FOMServer.World.Application.Handlers
{
    [PacketHandler]
    internal class RegisterClientHandler : PacketHandlerBase<RegisterClient>
    {
        private readonly IPlayerRegistry _playerRegistry;
        private readonly IClientPacketSender _clientPacketSender;
        private readonly ILogger<RegisterClientHandler> _logger;

        public RegisterClientHandler(
            IPlayerRegistry playerRegistry,
            IClientPacketSender clientPacketSender,
            ILogger<RegisterClientHandler> logger)
        {
            _playerRegistry = playerRegistry;
            _clientPacketSender = clientPacketSender;
            _logger = logger;
        }

        public override void Handle(NetworkAddress sender, in RegisterClient p)
        {
            var player = _playerRegistry.ClaimForClient(p.PlayerId, sender);
            if (player is null)
            {
                _logger.LogWarning("Client '{Address}' attempted to register unexpected player {PlayerId}", p.PlayerId, sender);
                return;
            }

            using var response = new PacketWriter<RegisterClientReturn>(sender);
            ref var rData = ref response.Data;

            rData.WorldId = p.WorldId;
            rData.PlayerId = p.PlayerId;
            rData.Status = RegisterClientReturn.StatusCode.Success;

            // Placeholder world-entry state; real values are sourced from the loaded Player
            // once DB-backed attribute/inventory loading lands (out of scope here).
            unsafe
            {
                rData.Avatar.Face = 5;
                rData.Avatar.Hair = 2;
                rData.Avatar.EquipmentSlots[(int)EquipmentSlot.Shirt] = 0;
                rData.Avatar.EquipmentSlots[(int)EquipmentSlot.Bottoms] = 0;
                rData.Avatar.EquipmentSlots[(int)EquipmentSlot.Shoes] = 0;

                rData.Attributes.Values[(int)AttributeType.Health] = 1000;
                rData.Attributes.Values[(int)AttributeType.Stamina] = 1000;
                rData.Attributes.Values[(int)AttributeType.BioEnergy] = 1000;
                rData.Attributes.Values[(int)AttributeType.Aura] = 1000;
                rData.Attributes.Values[(int)AttributeType.Agility] = 700;
            }

            rData.Profile.PlayerName = "Naruto Uzumaki";
            rData.NodeId = 1;

            _clientPacketSender.Send(response.Build());
        }
    }
}
