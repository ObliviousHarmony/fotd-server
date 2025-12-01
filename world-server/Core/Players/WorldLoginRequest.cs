namespace FOMServer.World.Core.Players
{
    public class WorldLoginRequest
    {
        public uint PlayerID { get; init; }
        public byte SelectedNodeID { get; init; }
    }
}
