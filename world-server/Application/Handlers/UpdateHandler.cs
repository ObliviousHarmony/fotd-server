using System.Diagnostics;
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
    public class UpdateHandler : BasePacketHandler<Update>
    {
        private readonly IPlayerService _playerService;
        private readonly IClientPacketSender _packetSender;

        private readonly Stopwatch _sendTimer = Stopwatch.StartNew();
        private readonly List<PlayerUpdateModel> _updates = new();

        public UpdateHandler(IPlayerService playerService, IClientPacketSender packetSender)
        {
            _packetSender = packetSender;
            _playerService = playerService;
        }

        public override void Handle(NetworkAddress sender, in Update p)
        {
            var player = _playerService.Get(sender);
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

                        if (_sendTimer.ElapsedMilliseconds < 100)
                            return;
                        _sendTimer.Restart();

                        Console.WriteLine(
    $@"Player {player.ID}
Position: {update.Placement.X}, {update.Placement.Y}, {update.Placement.Z}
Grid1: {p.Grid1} Grid2: {p.Grid2} VisibilityArea: {p.VisibilityAreaID}
Movement State: {update.MovementStateID}
Animating: {update.RawIsAnimating} Animation ID: {update.AnimationID}
Has Weapon: {update.RawHasWeaponEquipped}
Is Aiming: {update.RawIsWeaponAimed}
Is Firing: {update.RawIsWeaponFiring}"
);

                        using var response = new PacketBuilder<WorldUpdate>();
                        ref var rData = ref response.Data;

                        rData.PlayerID = player.ID;
                        rData.Update = update;
                        rData.Update.PlayerID = 2;

                        response.WithReliability(PacketReliability.UnreliableSequenced);
                        response.WithAddress(sender);
                        _packetSender.Send(response.Build());
                        break;
                    }
            }
        }
    }
}
