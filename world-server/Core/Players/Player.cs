using FOMServer.Shared.Core.DTOs;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Players;

namespace FOMServer.World.Core.Players
{
    public class Player : PlayerBase
    {
        private readonly AvatarDTO _avatar;
        private readonly PlayerAttributes _attributes;

        public Player(uint id, NetworkAddress clientAddress, AvatarDTO avatar, int[] attributeValues)
            : base(id, clientAddress)
        {
            _avatar = avatar;
            _attributes = new PlayerAttributes(this, attributeValues);
        }
    }
}
