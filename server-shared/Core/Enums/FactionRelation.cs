namespace FOMServer.Shared.Core.Enums
{
    /// <summary>
    /// An enum representing the different factions in the game.
    /// </summary>
    public enum FactionRelation : byte
    {
        INVALID_RELATION = 0,
        ALLY = 1,
        ECONOMIC_ALLY = 2,
        NEUTRAL = 3,
        ECONOMIC_ENEMY = 4,
        ENEMY = 5,
    }
}
