using FOMServer.Shared.Core.Packets.Types;

namespace FOMServer.Master.Core.Players
{
    internal class ClientSession
    {
        public ClientSession(NetworkAddress address)
        {
            Address = address;
        }

        public NetworkAddress Address { get; }
        public uint? PlayerID { get; private set; }
        public Player? Player { get; private set; }

        public bool IsLoggingIn => PlayerID.HasValue && Player is null;
        public bool IsReady => Player is not null;

        public void BeginLogin(uint playerID)
        {
            if (PlayerID is not null)
                throw new InvalidOperationException("Session login already started");

            PlayerID = playerID;
        }

        public void CompleteLogin(Player player)
        {
            if (player.ID != PlayerID)
                throw new InvalidOperationException(
                    $"Player ID {player.ID} does not match the session's login ID {PlayerID}");

            Player = player;
        }
    }
}
