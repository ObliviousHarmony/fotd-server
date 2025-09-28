using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Models.FOMData;

namespace FOMServer.World.Application.Networking
{
    public class MasterPacketSender : IMasterPacketSender
    {
        private readonly Func<MasterConnectionManager> networkManagerFactory;
        private MasterConnectionManager? networkManager;

        public MasterPacketSender(Func<MasterConnectionManager> networkManagerFactory)
        {
            this.networkManagerFactory = networkManagerFactory;
        }

        public void Send(PacketIdentifier id, FOMDataUnion data, PacketPriority priority, PacketReliability reliability, byte orderingChannel = 0)
        {
            if (networkManager == null)
                networkManager = networkManagerFactory();

            // Need to send via the master conncetion, store the address here for sending
            //networkManager.Send(id, data, 0, priority, reliability, orderingChannel);
        }
    }
}
