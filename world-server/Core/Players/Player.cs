using FOMServer.Shared.Core.DTOs;
using FOMServer.Shared.Core.Players;

namespace FOMServer.World.Core.Players
{
    public class Player : PlayerBase
    {
        private readonly AvatarDTO _avatar;
        private readonly PlayerAttributes _attributes;

        public Player(PlayerSession session, AvatarDTO avatar, PlayerAttributes attributes)
            : base(session)
        {
            _avatar = avatar;
            _attributes = attributes;
        }
    }
}
