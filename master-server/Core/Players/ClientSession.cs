using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Packets.Types;

namespace FOMServer.Master.Core.Players
{
    internal class ClientSession
    {
        private readonly Lock _syncRoot = new();
        private WorldID? _pendingWorld;

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
                {
                    return field;
                }
            }

            private set
            {
                lock (_syncRoot)
                {
                    field = value;
                }
            }
        }

        public Player? Player
        {
            get
            {
                lock (_syncRoot)
                {
                    return field;
                }
            }

            private set
            {
                lock (_syncRoot)
                {
                    field = value;
                }
            }
        }

        public WorldID? CurrentWorld
        {
            get
            {
                lock (_syncRoot)
                {
                    return field;
                }
            }

            private set
            {
                lock (_syncRoot)
                {
                    field = value;
                }
            }
        }

        public bool IsLoggingIn
        {
            get
            {
                lock (_syncRoot)
                {
                    return PlayerID.HasValue && Player is null;
                }
            }
        }

        public bool IsReady
        {
            get
            {
                lock (_syncRoot)
                {
                    return Player is not null;
                }
            }
        }

        public void BeginLogin(uint playerID)
        {
            lock (_syncRoot)
            {
                if (PlayerID is not null)
                {
                    throw new InvalidOperationException("Session login already started");
                }

                PlayerID = playerID;
            }
        }

        public void CompleteLogin(Player player)
        {
            lock (_syncRoot)
            {
                if (player.ID != PlayerID)
                {
                    throw new InvalidOperationException(
                        $"Player ID {player.ID} does not match the session's login ID {PlayerID}");
                }

                Player = player;
            }
        }

        public void BeginWorldTransfer(WorldID world)
        {
            if (world == WorldID.MasterServer)
            {
                throw new ArgumentException("Must use a valid WorldID", nameof(world));
            }

            lock (_syncRoot)
            {
                if (_pendingWorld.HasValue)
                {
                    throw new InvalidOperationException("A world transfer is already in progress");
                }

                _pendingWorld = world;
            }
        }

        public void CompleteWorldTransfer()
        {
            lock (_syncRoot)
            {
                if (!_pendingWorld.HasValue)
                {
                    throw new InvalidOperationException("There is no world transfer in progress");
                }

                CurrentWorld = _pendingWorld;
                _pendingWorld = null;
            }
        }
    }
}
