namespace FOMServer.Shared.Core.Networking
{
    public interface IPacketSender
    {
        void EnqueueSend(in QueuePacket packet);
    }
}
