using FOMServer.Shared.Core.Networking;

namespace FOMServer.World.Core.Networking
{
    public interface IMasterPacketSender
    {
        /// <summary>
        /// Sends a packet over the network.
        /// </summary>
        void Send(in QueuePacket packet);
    }
}
