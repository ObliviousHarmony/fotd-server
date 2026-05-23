using System.Runtime.InteropServices;
using FOMServer.Shared.Infrastructure.FOMNetwork;

namespace FOMServer.Shared.Services.FOMNetwork
{
    internal partial class ServerService : IServerService
    {
        public IntPtr Startup(ushort port) => FOMNetwork_Server_Startup(port);

        public void Shutdown(IntPtr server) => FOMNetwork_Server_Shutdown(server);

        [LibraryImport("FOMNetwork")]
        private static partial IntPtr FOMNetwork_Server_Startup(ushort port);

        [LibraryImport("FOMNetwork")]
        private static partial void FOMNetwork_Server_Shutdown(IntPtr server);
    }
}
