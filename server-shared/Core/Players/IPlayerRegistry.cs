using FOMServer.Shared.Core.Packets;

namespace FOMServer.Shared.Core.Players
{
    /// <summary>
    /// Registry for tracking online players.
    /// </summary>
    public interface IPlayerRegistry<TPlayer> where TPlayer : PlayerBase
    {
        /// <summary>
        /// Gets a player by their ID.
        /// </summary>
        TPlayer? Get(uint id);

        /// <summary>
        /// Gets a player by their client address.
        /// </summary>
        TPlayer? Get(NetworkAddress clientAddress);

        /// <summary>
        /// Registers a new player with the given ID and client address.
        /// </summary>
        TPlayer? Register(uint id, NetworkAddress clientAddress);

        /// <summary>
        /// Unregisters a player by their ID.
        /// </summary>
        void Unregister(uint id);
    }
}
