using FOMServer.Shared.Core.Players;

namespace FOMServer.World.Core.Players
{
    public class Player : PlayerBase
    {
        public PlayerAttributes Attributes { get; private set; } = null!;

        public void Init(int[] attributeValues)
        {
            Attributes = new PlayerAttributes(this, attributeValues);
        }
    }
}
