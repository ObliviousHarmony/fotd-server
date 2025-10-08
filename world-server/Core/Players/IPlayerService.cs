using FOMServer.Shared.Core.Packets;

namespace FOMServer.World.Core.Players
{
    public interface IPlayerService
    {
        /// <summary>
        /// Gets a registered player by their ID.
        /// </summary>
        Player? Get(uint id);

        /// <summary>
        /// Gets a registered player by their client address.
        /// </summary>
        Player? Get(NetworkAddress clientAddress);

        /// <summary>
        /// Records that a player is entering the world.
        /// </summary>
        Player? OnPlayerEnteringWorld(uint id, byte selectedNodeID);

        /// <summary>
        /// Records that a player has now entered the world.
        /// </summary>
        Player? OnPlayerEnteredWorld(uint id, NetworkAddress clientAddress);
    }
}
