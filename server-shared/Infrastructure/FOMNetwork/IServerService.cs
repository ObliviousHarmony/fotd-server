namespace FOMServer.Shared.Infrastructure.FOMNetwork
{
	public interface IServerService
	{
		/// <summary>
		/// Starts a server interface on the specified port.
		/// </summary>
		/// <param name="port">The port to listen on.</param>
		/// <returns>A pointer to the server interface, 0 if there was an error.</returns>
		nint Startup(ushort port);

		/// <summary>
		/// Shuts down the server interface.
		/// </summary>
		/// <param name="server">The server interface to shut down.</param>
		void Shutdown(nint server);
	}
}
