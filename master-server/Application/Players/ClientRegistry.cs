using System.Collections.Concurrent;
using FOMServer.Master.Core.Players;
using FOMServer.Shared.Interop.FOMNetwork;

namespace FOMServer.Master.Application.Players
{
    internal class ClientRegistry : IClientRegistry
    {
        private readonly ConcurrentDictionary<NetworkAddress, ClientSession> _sessions = new();
        private readonly ConcurrentDictionary<uint, ClientSession> _sessionsByPlayerId = new();

        public ClientSession? Get(NetworkAddress address)
        {
            return _sessions.GetValueOrDefault(address);
        }

        public ClientSession? Get(uint playerId)
        {
            return _sessionsByPlayerId.GetValueOrDefault(playerId);
        }

        public ClientSession Register(NetworkAddress address)
        {
            var session = new ClientSession(address);
            return !_sessions.TryAdd(address, session)
                ? throw new InvalidOperationException($"Client '{address}' is already registered")
                : session;
        }

        public void BeginLogin(ClientSession session, uint playerId)
        {
            session.BeginLogin(playerId);
            _sessionsByPlayerId[playerId] = session;
        }

        public bool Unregister(ClientSession session)
        {
            if (!_sessions.TryRemove(new(session.Address, session)))
            {
                return false;
            }

            if (session.PlayerId.HasValue)
            {
                _sessionsByPlayerId.TryRemove(new(session.PlayerId.Value, session));
            }

            return true;
        }
    }
}
