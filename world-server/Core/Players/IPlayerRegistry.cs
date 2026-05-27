using FOMServer.Shared.Core.Packets.Types;

namespace FOMServer.World.Core.Players
{
    internal interface IPlayerRegistry
    {
        Player? Get(uint playerId);

        Player? Get(NetworkAddress address);

        Player PrepareForClient(uint playerId, uint clientBinaryAddress);

        Player? ClaimForClient(uint playerId, NetworkAddress sender);

        void Logout(Player player);
    }
}
