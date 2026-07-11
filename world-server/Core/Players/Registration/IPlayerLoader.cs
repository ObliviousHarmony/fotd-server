namespace FOMServer.World.Core.Players.Registration
{
    internal interface IPlayerLoader
    {
        Player? Load(uint id);
    }
}
