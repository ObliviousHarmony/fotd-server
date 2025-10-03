using FOMServer.Master.Application.FOMPacket;
using FOMServer.Master.Core.Networking;
using FOMServer.Master.Core.Players;
using FOMServer.Shared.Core;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket;
using FOMServer.Shared.Core.FOMPacket.Data;
using FOMServer.Shared.Core.Handlers;

namespace FOMServer.Master.Application.Handlers
{
    public class CreateCharacterHandler : PacketHandler<CreateCharacter>
    {
        private readonly IClientPacketSender _packetSender;
        private readonly IPlayerService _playerService;
        private readonly ICharacterRepository _characterRepository;
        private readonly IWorldOverviewFactory _worldOverviewFactory;

        public CreateCharacterHandler(IClientPacketSender packetSender, IPlayerService playerService, ICharacterRepository characterRepository, IWorldOverviewFactory worldOverviewFactory)
        {
            this._packetSender = packetSender;
            this._playerService = playerService;
            this._characterRepository = characterRepository;
            this._worldOverviewFactory = worldOverviewFactory;
        }

        public override PacketIdentifier PacketID => PacketIdentifier.ID_CREATE_CHARACTER;

        public override void Handle(NetworkAddress sender, in CreateCharacter data)
        {
            var player = _playerService.Get(sender);
            if (player == null)
                return;

            var created = _characterRepository.Create(
                player.ID,
                data.Avatar.Faction,
                data.Name,
                data.Biography,
                data.Avatar.Sex,
                data.Avatar.SkinColor,
                data.Avatar.Face,
                data.Avatar.Hair
            );
            if (created == null)
                throw new InvalidOperationException("Failed to create character.");

            player.HasCharacter = true;

            var response = new LoginReturn()
            {
                Status = LoginReturn.StatusCode.LOGIN_RETURN_SUCCESS,
                PlayerID = player.ID,
                AccountType = 3,
                IsVolunteer = false,
                ClientVersion = GlobalConstants.ClientVersion,
                WorldOverview = this._worldOverviewFactory.Create(player),
            };

            _packetSender.Send(
                PacketIdentifier.ID_LOGIN_RETURN,
                new FOMDataUnion { LoginReturn = response },
                sender,
                PacketPriority.HIGH_PRIORITY,
                PacketReliability.RELIABLE_ORDERED
            );
        }
    }
}
