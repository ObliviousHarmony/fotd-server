namespace FOMServer.Shared.Core.Enums
{
    public enum WorldUpdateType : byte
    {
        Invalid = 0, // WORLDUPDATE_INVALID
        Player = 1, // WORLDUPDATE_PLAYER
        Neighbor = 2, // WORLDUPDATE_NEIGHBOR
        Enemy = 3, // WORLDUPDATE_ENEMY
        NeighborEnemy = 4, // WORLDUPDATE_NEIGHBOR_ENEMY
    }
}
