using System;
using System.Net.Sockets;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Packets.RakNet;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Metadata;
using FOMServer.World.Core.Networking;

namespace FOMServer.World.Application.Handlers
{
    [PacketHandler]
    internal class RegisterClientHandler : PacketHandlerBase<RegisterClient>
    {
        private readonly IClientPacketSender _packetSender;

        public RegisterClientHandler(IClientPacketSender packetSender)
        {
            _packetSender = packetSender;
        }

        public override void Handle(NetworkAddress sender, in RegisterClient p)
        {
            using var response = new PacketWriter<RegisterClientReturn>(sender);
            ref var rData = ref response.Data;

            rData.WorldID = p.WorldID;
            rData.PlayerID = p.PlayerID;
            rData.Status = RegisterClientReturn.StatusCode.Success;

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
            rData.NodeID = 1;

            _packetSender.Send(response.Build());
        }
    }
}
