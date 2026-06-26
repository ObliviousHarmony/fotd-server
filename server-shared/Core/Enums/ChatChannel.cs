namespace FOMServer.Shared.Core.Enums
{
    public enum ChatChannel : byte
    {
        General = 0, // CHAT_GENERAL
        Private = 1, // CHAT_PRIVATE
        Faction = 2, // CHAT_FACTION
        DollyInc = 3, // CHAT_DOLLYINC
        Department = 4, // CHAT_DEPARTMENT
        Mission = 5, // CHAT_MISSION
        Global = 6, // CHAT_GLOBAL
        Help = 7, // CHAT_HELP
        Staff = 8, // CHAT_STAFF
        Trade = 9, // CHAT_TRADE
        System = 10, // CHAT_SYSTEM
        Local = 11, // CHAT_LOCAL
        GM = 12, // CHAT_GM

        NUM_CHAT_CHANNELS = 13,
    }
}
