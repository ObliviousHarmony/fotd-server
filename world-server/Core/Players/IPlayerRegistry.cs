using FOMServer.Shared.Core.Packets.Types;

namespace FOMServer.World.Core.Players
{
    internal interface IPlayerRegistry
    {
        Player? Get(uint playerID);

        Player? Get(NetworkAddress address);

        Player PrepareForClient(uint playerID, uint clientBinaryAddress);

        Player? ClaimForClient(uint playerID, NetworkAddress sender);

        void Logout(Player player);
    }
}
