using System.Security.Principal;
using FOMServer.Master.Core.Networking;
using FOMServer.Shared.Core.Constants;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Core.Repositories;
using FOMServer.Shared.Metadata;

namespace FOMServer.Master.Application.Handlers
{
    [PacketHandler]
    internal class CreateCharacterHandler : PacketHandlerBase<CreateCharacter>
    {
        private readonly IClientPacketSender _packetSender;
        private readonly IPlayerRepository _playerRepository;

        public CreateCharacterHandler(
            IClientPacketSender packetSender,
            IPlayerRepository playerRepository)
        {
            _packetSender = packetSender;
            _playerRepository = playerRepository;
        }

        public override void Handle(NetworkAddress sender, in CreateCharacter p)
        {
            using var response = new PacketWriter<LoginReturn>(sender);
            ref var rData = ref response.Data;

            rData.PlayerID = p.PlayerID;

            var player = _playerRepository.GetByName(p.Name);
            if (player != null)
            {
                rData.Status = LoginReturn.StatusCode.CreateCharacterError;
                _packetSender.Send(response.Build());
                return;
            }

            player = _playerRepository.Create(
                p.PlayerID,
                p.Name,
                p.Biography,
                p.Avatar.Sex,
                p.Avatar.Race,
                p.Avatar.Face,
                p.Avatar.Hair
             );

            if (player == null)
            {
                rData.Status = LoginReturn.StatusCode.CreateCharacterError;
                _packetSender.Send(response.Build());
                return;
            }

            rData.Status = LoginReturn.StatusCode.Success;

            // Populate Login Return

            _packetSender.Send(response.Build());
        }
    }
}
