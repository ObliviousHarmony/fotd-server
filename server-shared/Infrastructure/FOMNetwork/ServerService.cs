using FOMServer.Shared.Infrastructure.FOMNetwork;
using System.Runtime.InteropServices;

namespace FOMServer.Shared.Services.FOMNetwork
{
	public partial class ServerService : IServerService
	{
		/// <inheritdoc />
		public IntPtr Startup(ushort port) => FOMNetwork_Server_Startup(port);

		/// <inheritdoc />
		public void Shutdown(IntPtr server) => FOMNetwork_Server_Shutdown(server);

		[LibraryImport("FOMNetwork")]
		private static partial IntPtr FOMNetwork_Server_Startup(ushort port);

		[LibraryImport("FOMNetwork")]
		private static partial void FOMNetwork_Server_Shutdown(IntPtr server);
	}
}
