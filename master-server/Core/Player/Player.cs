using FOMServer.Shared.Core.Player;

namespace FOMServer.Master.Core.Player
{
    public class Player : PlayerBase
    {
        private readonly string _username;

        public Player(PlayerSession session, string username)
            : base(session)
        {
            _username = username;
        }
    }
}
