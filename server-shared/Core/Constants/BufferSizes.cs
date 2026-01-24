namespace FOMServer.Shared.Core.Constants
{
    /// <summary>
    /// A reference for the sizes of common fixed-length buffers.
    /// </summary>
    public static class BufferSizes
    {
        public const int Username = 19; // USERNAME
        public const int PlayerName = 20; // PLAYER_NAME
        public const int PlayerBiography = 511; // PLAYER_BIOGRAPHY
        public const int FactionName = 32; // FACTION_NAME
        public const int RankName = 32; // RANK_NAME
        public const int MaxItemListSize = 60000; // MAX_ITEM_LIST_SIZE
    }
}
