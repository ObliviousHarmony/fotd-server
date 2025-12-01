using FOMServer.Shared.Core.Packets;

namespace FOMServer.World.Core.Players
{
    public interface IWorldLoginService
    {
        /// <summary>
        /// Prepares a pending world login request.
        /// </summary>
        void Prepare(uint playerID, byte selectedNodeID);

        /// <summary>
        /// Logs in a player: validates the pending request, registers the player, and sets up persistence.
        /// </summary>
        /// <returns>The login result, or null if no pending request exists.</returns>
        WorldLoginResult? Login(uint playerID, NetworkAddress clientAddress);
    }
}
