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
        private readonly IPlayerRegistry _playerRegistry;
        private readonly IPlayerRepository _playerRepository;
        private readonly IWorldOverviewFactory _worldOverviewFactory;

        public CreateCharacterHandler(
            IClientPacketSender packetSender,
            IPlayerRegistry playerRegistry,
            IPlayerRepository playerRepository,
            IWorldOverviewFactory worldOverviewFactory
        )
        {
            _packetSender = packetSender;
            _playerRegistry = playerRegistry;
            _playerRepository = playerRepository;
            _worldOverviewFactory = worldOverviewFactory;
        }

        public override void Handle(NetworkAddress sender, in CreateCharacter p)
        {
            var player = _playerRegistry.Get(sender);
            if (player == null)
                throw new InvalidOperationException($"Player not found for address {sender}");

            var created = _playerRepository.CreateAvatar(
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
                throw new InvalidOperationException("Failed to create avatar");

            player.SetAvatar(created);

            using var response = new PacketWriter<LoginReturn>();
            ref var rData = ref response.Data;

            rData.Status = LoginReturn.StatusCode.LOGIN_RETURN_SUCCESS;
            rData.PlayerID = player.ID;
            rData.AccountType = 3;
            rData.IsVolunteer = false;
            rData.ClientVersion = GlobalConstants.ClientVersion;
            rData.WorldOverview = _worldOverviewFactory.Create(player);

            response.AddDestination(sender);
            _packetSender.Send(response.Build());
        }
    }
}
