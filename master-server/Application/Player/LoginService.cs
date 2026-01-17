using FOMServer.Master.Core.Player;
using FOMServer.Shared.Core.Packets.Types;

namespace FOMServer.Master.Application.Player
{
    public class LoginService : ILoginService
    {
        private readonly ILoginRepository _loginRepository;
        private readonly IPlayerRegistry _playerRegistry;

        public LoginService(ILoginRepository loginRepository, IPlayerRegistry playerRegistry)
        {
            _loginRepository = loginRepository;
            _playerRegistry = playerRegistry;
        }

        public Player? Login(string username, string password, NetworkAddress clientAddress)
        {
            var playerID = _loginRepository.TryLogin(username, password);
            if (playerID == null)
                return null;

            return _playerRegistry.Register(playerID.Value, clientAddress);
        }

        public bool Logout(Player player)
        {
            if (_playerRegistry.Get(player.ID) == null)
                return false;

            _playerRegistry.Unregister(player.ID);
            _loginRepository.Logout(player.ID);

            return true;
        }
    }
}
