using FOMServer.Shared.Core.DTOs;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Players;

namespace FOMServer.Master.Core.Players
{
    public class Player : PlayerBase
    {
        private readonly string _username;
        private AvatarDTO? _avatar;

        public Player(
            uint id,
            NetworkAddress clientAddress,
            string username,
            AvatarDTO? avatar)
            : base(id, clientAddress)
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
