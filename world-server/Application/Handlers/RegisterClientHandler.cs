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
        private readonly IClientPacketSender _packetSender;
        private readonly IPlayerRegistry _playerRegistry;
        private readonly IWorldLoginService _worldLoginService;

        public RegisterClientHandler(
            IClientPacketSender packetSender,
            IPlayerRegistry playerRegistry,
            IWorldLoginService worldLoginService)
        {
            _packetSender = packetSender;
            _playerRegistry = playerRegistry;
            _worldLoginService = worldLoginService;
        }

        public override void Handle(NetworkAddress sender, in RegisterClient p)
        {
            var request = _worldLoginService.GetAndRemove(p.PlayerID);
            if (request == null)
                throw new InvalidOperationException($"No pending world login request for player {p.PlayerID}");

            var player = _playerRegistry.Register(p.PlayerID, sender);
            if (player == null)
                throw new InvalidOperationException($"Failed to register player {p.PlayerID}");

            using var response = new PacketWriter<RegisterClientReturn>();
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
            rData.SelectedNode = request.SelectedNodeID;

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

            rData.Equipment[(int)EquipmentSlot.Back] = new ItemSlotModel()
            {
                InUse = true,
                Item = new ItemModel()
                {
                    ID = itemID++,
                    Type = (ItemType)104,
                    Value = 200,
                    Durability = 15000,
                    IsFactionItem = false,
                }
            };

            response.AddDestination(sender);
            _packetSender.Send(response.Build());
        }
    }
}
