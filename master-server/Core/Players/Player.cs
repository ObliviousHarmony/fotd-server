using FOMServer.Shared.Core.Players;

namespace FOMServer.Master.Core.Players
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
