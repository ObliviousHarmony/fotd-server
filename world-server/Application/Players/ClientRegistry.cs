using System.Collections.Concurrent;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.World.Core.Players;

namespace FOMServer.World.Application.Players
{
    internal class ClientRegistry : IClientRegistry
    {
        private readonly ConcurrentDictionary<NetworkAddress, ClientSession> _sessions = new();
        private readonly ConcurrentDictionary<uint, ClientSession> _sessionsByPlayerID = new();

        public ClientSession? Get(NetworkAddress address)
        {
            return _sessions.GetValueOrDefault(address);
        }

        public ClientSession? Get(uint playerID)
        {
            return _sessionsByPlayerID.GetValueOrDefault(playerID);
        }

        public ClientSession Register(NetworkAddress address)
        {
            var session = new ClientSession(address);
            return !_sessions.TryAdd(address, session)
                ? throw new InvalidOperationException($"Client {address} is already registered")
                : session;
        }

        public void BeginLogin(ClientSession session, uint playerID)
        {
            session.BeginLogin(playerID);
            _sessionsByPlayerID[playerID] = session;
        }

        public bool Unregister(ClientSession session)
        {
            if (!_sessions.TryRemove(new KeyValuePair<NetworkAddress, ClientSession>(session.Address, session)))
            {
                return false;
            }

            if (session.PlayerID.HasValue)
            {
                _ = _sessionsByPlayerID.TryRemove(new KeyValuePair<uint, ClientSession>(session.PlayerID.Value, session));
            }

            return true;
        }
    }
}
