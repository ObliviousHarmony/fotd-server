namespace FOMServer.Master.Core.Interfaces
{
    public interface ICharacterRepository
    {
        /// <summary>
        /// Checks to see if a name is taken and returns the ID of the account that has it if so.
        /// </summary>
        uint? IsNameTaken(string name);
    }
}
