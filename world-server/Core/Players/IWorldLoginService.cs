namespace FOMServer.World.Core.Players
{
    public interface IWorldLoginService
    {
        /// <summary>
        /// Adds a pending world login request.
        /// </summary>
        void AddRequest(uint playerID, byte selectedNodeID);

        /// <summary>
        /// Gets and removes a pending request, or returns null if not found.
        /// </summary>
        WorldLoginRequest? GetAndRemove(uint playerID);
    }
}
