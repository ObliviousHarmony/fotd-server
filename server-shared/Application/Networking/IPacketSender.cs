namespace FOMServer.Shared.Core.Networking
{
    /// <summary>
    /// Describes a service that can send packets over the network.
    /// </summary>
    public interface IPacketSender
    {
        /// <summary>
        /// Enqueues a packet to be sent over the network.
        /// </summary>
        void EnqueueSend(in QueuePacket packet);
    }
}
