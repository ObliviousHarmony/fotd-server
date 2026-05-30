using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Packets.Types;

namespace FOMServer.Master.Core.Players
{
    internal class ClientSession
    {
        private readonly Lock _syncRoot = new();

        public ClientSession(NetworkAddress address)
        {
            Address = address;
        }

        public NetworkAddress Address { get; }

        public uint? PlayerId
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

        public WorldId? CurrentWorld
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

        public WorldId? PendingWorld
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
                    return PlayerId.HasValue && Player is null;
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

        public void BeginLogin(uint playerId)
        {
            lock (_syncRoot)
            {
                if (PlayerId is not null)
                {
                    throw new InvalidOperationException("Session login already started");
                }

                PlayerId = playerId;
            }
        }

        public void CompleteLogin(Player player)
        {
            lock (_syncRoot)
            {
                if (player.Id != PlayerId)
                {
                    throw new InvalidOperationException(
                        $"Player Id {player.Id} does not match the session's login Id {PlayerId}");
                }

                Player = player;
            }
        }

        public void BeginWorldTransfer(WorldId world)
        {
            if (world == WorldId.MasterServer)
            {
                throw new ArgumentException("Must use a valid WorldId", nameof(world));
            }

            lock (_syncRoot)
            {
                if (PendingWorld.HasValue)
                {
                    throw new InvalidOperationException("A world transfer is already in progress");
                }

                PendingWorld = world;
            }
        }

        public void CompleteWorldTransfer()
        {
            lock (_syncRoot)
            {
                if (!PendingWorld.HasValue)
                {
                    throw new InvalidOperationException("There is no world transfer in progress");
                }

                CurrentWorld = PendingWorld;
                PendingWorld = null;
            }
        }

        public void AbortWorldTransfer()
        {
            lock (_syncRoot)
            {
                if (!PendingWorld.HasValue)
                {
                    throw new InvalidOperationException("There is no world transfer in progress");
                }

                PendingWorld = null;
            }
        }
    }
}
