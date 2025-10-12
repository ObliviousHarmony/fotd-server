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

            using var response = new PacketBuilder<RegisterClientReturn>();
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
            rData.Attributes[PlayerAttribute.Agility] = 1000;
            rData.Name = "Oblivious Test";
            rData.SelectedNode = player.SelectedNodeID;

            // Item Types
            // 1 - 49: Weapons
            // 50 - 99: Ammunition
            // 100 - 109: Implants
            // 110 - ?: Armor

            // Item Testing
            uint itemID = 1;
            int itemIndex = 0;
            rData.InventoryItems[itemIndex++] = new ItemModel()
            {
                ID = itemID++,
                Type = (ItemType)400,
                Value = 10,
                Durability = 5000,
                IsFactionItem = false,
            };
            rData.NumInventoryItems = (byte)itemIndex;

            rData.Weapons[0] = new ItemSlotModel()
            {
                InUse = true,
                Item = new ItemModel()
                {
                    ID = itemID++,
                    Type = (ItemType)3,
                    Value = 100,
                    Durability = 10000,
                    IsFactionItem = false,
                }
            };

            rData.Equipment[(int)EquipmentSlot.Face] = new ItemSlotModel()
            {
                InUse = true,
                Item = new ItemModel()
                {
                    ID = itemID++,
                    Type = (ItemType)100,
                    Value = 200,
                    Durability = 15000,
                    IsFactionItem = false,
                }
            };

            response.WithAddress(sender);
            _packetSender.Send(response.Build());
        }
    }
}
