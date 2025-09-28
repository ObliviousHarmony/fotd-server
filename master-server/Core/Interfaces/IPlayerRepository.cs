namespace FOMServer.Master.Core.Interfaces
{
    public interface IPlayerRepository
    {
        /// <summary>
        /// Attempts to find a player by their name.
        /// </summary>
        uint? FindIDByName(string name);
    }
}
