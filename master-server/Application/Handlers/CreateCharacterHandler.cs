using FOMServer.Master.Application.Packets;
using FOMServer.Master.Core.Networking;
using FOMServer.Master.Core.Players;
using FOMServer.Shared.Core.Constants;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Packets.Data;
using FOMServer.Shared.Metadata;

namespace FOMServer.Master.Application.Handlers
{
    [PacketHandler]
    public class CreateCharacterHandler : BasePacketHandler<CreateCharacter>
    {
        private readonly IClientPacketSender _packetSender;
        private readonly IPlayerService _playerService;
        private readonly ICharacterRepository _characterRepository;
        private readonly IWorldOverviewFactory _worldOverviewFactory;

        public CreateCharacterHandler(
            IClientPacketSender packetSender,
            IPlayerService playerService,
            ICharacterRepository characterRepository,
            IWorldOverviewFactory worldOverviewFactory
        )
        {
            _packetSender = packetSender;
            _playerService = playerService;
            _characterRepository = characterRepository;
            _worldOverviewFactory = worldOverviewFactory;
        }

        public override void Handle(NetworkAddress sender, in CreateCharacter p)
        {
            var player = _playerService.Get(sender);
            if (player == null)
                throw new InvalidOperationException($"Player not found for address {sender}");

            var created = _characterRepository.Create(
                player.ID,
                p.Avatar.Faction,
                p.Name,
                p.Biography,
                p.Avatar.Sex,
                p.Avatar.SkinColor,
                p.Avatar.Face,
                p.Avatar.Hair
            );
            if (created == null)
                throw new InvalidOperationException("Failed to create character");

            player.HasCharacter = true;

            using var response = new PacketBuilder<LoginReturn>();
            ref var rData = ref response.Data;

            rData.Status = LoginReturn.StatusCode.LOGIN_RETURN_SUCCESS;
            rData.PlayerID = player.ID;
            rData.AccountType = 3;
            rData.IsVolunteer = false;
            rData.ClientVersion = GlobalConstants.ClientVersion;
            rData.WorldOverview = _worldOverviewFactory.Create(player);

            response.WithAddress(sender);
            _packetSender.Send(response.Build());
        }
    }
}
