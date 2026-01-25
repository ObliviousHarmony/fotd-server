using System.Net;
using System.Net.Sockets;
using FOMServer.Shared.Core.Enums;

namespace FOMServer.World.Core
{
    public class ServerSettings
    {
        private string? _clientIP;

        public WorldID[] WorldIDs { get; init; } = [];
        public string MasterServerHost { get; init; } = null!;
        public string ClientHost { get; init; } = null!;

        public string? ClientIP
        {
            get
            {
                if (_clientIP != null)
                    return _clientIP;

                var hostAddresses = Dns.GetHostAddresses(ClientHost, AddressFamily.InterNetwork);
                if (hostAddresses == null)
                    return null;

                var ipAddress = hostAddresses.FirstOrDefault();
                if (ipAddress == null)
                    return null;

                _clientIP = ipAddress.ToString();
                return _clientIP;
            }
        }
    }

    public class DatabaseSettings
    {
        public string Name { get; init; } = null!;
        public string ConnectionString { get; init; } = null!;
    }
}
