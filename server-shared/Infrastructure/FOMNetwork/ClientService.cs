using System.Runtime.InteropServices;
using FOMServer.Shared.Interop.FOMNetwork;

namespace FOMServer.Shared.Services.FOMNetwork
{
    internal partial class ClientService : IClientService
    {
        public IntPtr Connect(string hostAddress, ushort port, int threadSleepMs)
        {
            return FOMNetwork_Client_Connect(hostAddress, port, threadSleepMs);
        }

        public void Disconnect(IntPtr client)
        {
            FOMNetwork_Client_Disconnect(client);
        }

        [LibraryImport("FOMNetwork", StringMarshalling = StringMarshalling.Utf8)]
        private static partial IntPtr FOMNetwork_Client_Connect(string hostAddress, ushort port, int threadSleepMs);

        [LibraryImport("FOMNetwork")]
        private static partial void FOMNetwork_Client_Disconnect(IntPtr client);
    }
}
