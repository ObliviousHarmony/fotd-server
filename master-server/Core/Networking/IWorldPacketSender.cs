using FOMServer.Shared.Core.Networking;

namespace FOMServer.Master.Core.Networking
{
    public interface IWorldPacketSender
    {
        /// <summary>
        /// Sends a packet over the network.
        /// </summary>
        void Send(in QueuePacket packet);

        /// <summary>
        /// Broadcast a packet to all connected clients.
        /// </summary>
        void Broadcast(in QueuePacket packet);
    }
}
