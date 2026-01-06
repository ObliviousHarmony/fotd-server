using FOMServer.Shared.Core.DTOs;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Players;

namespace FOMServer.Master.Core.Players
{
    public interface IPlayerRepository : IPlayerRepositoryBase
    {
        uint? GetIDByUsername(string username);
        uint? GetIDByName(string name);

        AvatarDTO? CreateAvatar(
            uint playerID,
            string name,
            string biography,
            AvatarSex sex,
            AvatarSkin skinColor,
            byte face,
            byte hair
        );
    }
}
