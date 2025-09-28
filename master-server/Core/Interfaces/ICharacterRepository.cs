using FOMServer.Master.Core.DTOs;
using FOMServer.Shared.Core.Enums;

namespace FOMServer.Master.Core.Interfaces
{
    public interface ICharacterRepository
    {
        /// <summary>
        /// Checks to see if a character already exists with the given name.
        /// </summary>
        uint? Exists(string name);

        /// <summary>
        /// Loads the character for the given account ID.
        /// </summary>
        CharacterDto? Get(uint accountID);

        /// <summary>
        /// Creates a new character for the given account.
        /// </summary>
        CharacterDto? Create(
            uint accountID,
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
