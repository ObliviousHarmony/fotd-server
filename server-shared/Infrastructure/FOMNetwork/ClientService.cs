using FOMServer.Shared.Infrastructure.FOMNetwork;
using System.Runtime.InteropServices;

namespace FOMServer.Shared.Services.FOMNetwork
{
	public partial class ClientService : IClientService
	{
		/// <inheritdoc />
		public IntPtr Connect(string hostAddress, ushort port) => FOMNetwork_Client_Connect(hostAddress, port);

		/// <inheritdoc />
		public void Disconnect(IntPtr client) => FOMNetwork_Client_Disconnect(client);

		[LibraryImport("FOMNetwork", StringMarshalling = StringMarshalling.Utf8)]
		private static partial IntPtr FOMNetwork_Client_Connect(string hostAddress, ushort port);

		[LibraryImport("FOMNetwork")]
		private static partial void FOMNetwork_Client_Disconnect(IntPtr client);
	}
}
