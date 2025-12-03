namespace FOMServer.World.Core.Players
{
    public class LoginContext
    {
        public required Player Player { get; init; }
        public required byte SelectedNodeID { get; init; }
    }
}
