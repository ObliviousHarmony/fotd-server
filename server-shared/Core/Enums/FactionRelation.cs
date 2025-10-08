namespace FOMServer.Shared.Core.Enums
{
    /// <summary>
    /// An enum representing the different factions in the game.
    /// </summary>
    public enum FactionRelation : byte
    {
        Invalid = 0, // INVALID_RELATION
        Ally = 1, // ALLY
        EconomicAlly = 2, // ECONOMIC_ALLY
        Neutral = 3, // NEUTRAL
        EconomicEnemy = 4, // ECONOMIC_ENEMY
        Enemy = 5, // ENEMY
        NUM_RELATIONS = 6,
    }
}
