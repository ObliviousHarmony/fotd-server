namespace FOMServer.World.Core.Players
{
    internal interface IPlayerLoader
    {
        Player? Load(uint id);
    }
}
