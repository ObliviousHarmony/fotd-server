using FOMServer.Shared.Core.Packets.Types;

namespace FOMServer.World.Core.Players
{
    internal class ClientSession
    {
        private readonly Lock _syncRoot = new();

        public ClientSession(NetworkAddress address)
        {
            Address = address;
        }

        public NetworkAddress Address { get; }

        public uint? PlayerID
        {
            get
            {
                lock (_syncRoot)
                    return field;
            }

            private set
            {
                lock (_syncRoot)
                    field = value;
            }
        }

        public Player? Player
        {
            get
            {
                lock (_syncRoot)
                    return field;
            }

            private set
            {
                lock (_syncRoot)
                    field = value;
            }
        }

        public bool IsLoggingIn
        {
            get
            {
                lock (_syncRoot)
                    return PlayerID.HasValue && Player is null;
            }
        }

        public bool IsReady
        {
            get
            {
                lock (_syncRoot)
                    return Player is not null;
            }
        }

        public void BeginLogin(uint playerID)
        {
            lock (_syncRoot)
            {
                if (PlayerID is not null)
                    throw new InvalidOperationException("Session login already started");

                PlayerID = playerID;
            }
        }

        public void CompleteLogin(Player player)
        {
            lock (_syncRoot)
            {
                if (player.ID != PlayerID)
                    throw new InvalidOperationException(
                        $"Player ID {player.ID} does not match the session's login ID {PlayerID}");

                Player = player;
            }
        }
    }
}
