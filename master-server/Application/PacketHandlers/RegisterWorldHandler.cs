using FOMServer.Master.Application.Networking;
using FOMServer.Master.Core.Interfaces;
using FOMServer.Shared.Application.PacketHandlers;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Models;
using FOMServer.Shared.Core.Models.FOMData;

namespace FOMServer.Master.Application.PacketHandlers
{
    public class RegisterWorldPacketHandler : PacketHandler<RegisterWorld>
    {
        public override PacketIdentifier PacketID => PacketIdentifier.ID_REGISTER_WORLD;

        public RegisterWorldPacketHandler()
        {
        }

        public override void Handle(NetworkAddress sender, in RegisterWorld data)
        {
            Console.WriteLine($"[World] Registered World {data.WorldID} at {data.Address}:{data.Port}");
        }
    }
}
