using FOMServer.Shared.Interop.FOMNetwork.Structs;

namespace FOMServer.Shared.Interop.FOMNetwork.Constants
{
    public static class AvatarConstants
    {
        public static readonly uint NumHairstyles = 23;

        public static readonly Race[] FaceMap =
        [
            Race.Black, // 0
            Race.Black, // 1
            Race.Black, // 2
            Race.Black, // 3
            Race.Black, // 4
            Race.White, // 5
            Race.White, // 6
            Race.White, // 7
            Race.White, // 8
            Race.White, // 9
            Race.White, // 10
            Race.White, // 11
            Race.White, // 12
            Race.White, // 13
            Race.White, // 14
            Race.White, // 15
            Race.White, // 16
            Race.White, // 17
            Race.White, // 18
            Race.White, // 19
            Race.Black, // 20
            Race.Black, // 21
            Race.Black, // 22
            Race.White, // 23
            Race.White, // 24
            Race.White, // 25
            Race.White, // 26
            Race.White, // 27
        ];

        public enum Sex : byte
        {
            Male = 0, // MALE
            Female = 1, // FEMALE
        }

        public enum Race : byte
        {
            White = 0, // WHITE
            Black = 1, // BLACK
        }

        public static bool IsValidAvatar(Race race, Sex sex, uint face, uint hair)
        {
            if (face >= FaceMap.Length)
            {
                return false;
            }

            if (FaceMap[face] != race)
            {
                return false;
            }

            if (hair >= NumHairstyles)
            {
                return false;
            }

            return true;
        }
    }
}
