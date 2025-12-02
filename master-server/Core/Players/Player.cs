using FOMServer.Shared.Core.DTOs;
using FOMServer.Shared.Core.Players;

namespace FOMServer.Master.Core.Players
{
    public class Player : PlayerBase
    {
        private readonly string _username;
        private AvatarDTO? _avatar;

        public Player(PlayerSession session, string username, AvatarDTO? avatar)
            : base(session)
        {
            _username = username;
            _avatar = avatar;
        }

        public bool HasAvatar => _avatar != null;

        public void SetAvatar(AvatarDTO avatar)
        {
            if (_avatar != null)
                throw new InvalidOperationException("Player already has an avatar");

            _avatar = avatar;
        }
    }
}
