using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Packets.Data;
using FOMServer.Shared.Metadata;
using FOMServer.World.Core.Networking;
using FOMServer.World.Core.Players;

namespace FOMServer.World.Application.Handlers
{
    [PacketHandler]
    public class UpdateHandler : PacketHandlerBase<Update>
    {
        private readonly IPlayerRegistry _playerRegistry;
        private readonly IClientPacketSender _packetSender;

        public UpdateHandler(IPlayerRegistry playerRegistry, IClientPacketSender packetSender)
        {
            _packetSender = packetSender;
            _playerRegistry = playerRegistry;
        }

        public override void Handle(NetworkAddress sender, in Update p)
        {
            var player = _playerRegistry.Get(sender);
            if (player == null)
                throw new InvalidOperationException($"Player at {sender} not found");

            switch (p.Type)
            {
                case WorldUpdateType.Player:
                    {
                        ref readonly var data = ref p.Data.Player;
                        ref readonly var update = ref data.Update;
                        if (player.ID != update.PlayerID)
                            throw new InvalidOperationException($"Player {player.ID} Provided Wrong ID: {update.PlayerID}");

                        using var response = new PacketWriter<WorldUpdate>();
                        ref var rData = ref response.Data;

                        // Clone us so that we can test the update handling.
                        rData.PlayerID = player.ID;
                        rData.NumUpdates = 1;
                        rData.Updates[0].Type = WorldUpdateType.Neighbor;
                        rData.Updates[0].Player = update;
                        rData.Updates[0].Player.PlayerID = 2;

                        response.AddDestination(sender);
                        _packetSender.Send(response.Build());
                        break;
                    }
            }
        }
    }
}
