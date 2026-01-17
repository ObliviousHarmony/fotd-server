namespace FOMServer.World.Core.Player
{
    public class LoginContext
    {
        public required Player Player { get; init; }
        public required byte SelectedNodeID { get; init; }
    }
}
