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
    public class RegisterClientHandler : PacketHandlerBase<RegisterClient>
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

            _packetSender.Send(response.Build());
        }
    }
}
