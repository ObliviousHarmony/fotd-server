using FOMServer.Master.Core.DTOs;
using FOMServer.Shared.Core.Enums;

namespace FOMServer.Master.Core.Players
{
    public interface ICharacterRepository
    {
        /// <summary>
        /// Checks to see if a character already exists with the given name.
        /// </summary>
        uint? Exists(string name);

        /// <summary>
        /// Loads the character for the given player ID.
        /// </summary>
        CharacterDto? Get(uint playerID);

        /// <summary>
        /// Creates a new character for the given player.
        /// </summary>
        CharacterDto? Create(
            uint playerID,
            Faction faction,
            string name,
            string biography,
            AvatarSex sex,
            AvatarSkin skinColor,
            byte face,
            byte hair
        );
    }
}
