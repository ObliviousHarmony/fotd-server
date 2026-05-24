using System.Net;
using System.Net.Sockets;
using FOMServer.Shared.Core.Enums;

namespace FOMServer.World.Core
{
    internal class ServerSettings
    {
        private string? _clientIP;

        public WorldID[] WorldIDs { get; init; } = [];
        public string MasterServerHost { get; init; } = null!;
        public string ClientHost { get; init; } = null!;

        public string? ClientIP
        {
            get
            {
                if (_clientIP is not null)
                    return _clientIP;

                var hostAddresses = Dns.GetHostAddresses(ClientHost, AddressFamily.InterNetwork);
                if (hostAddresses is null)
                    return null;

                var ipAddress = hostAddresses.FirstOrDefault();
                if (ipAddress is null)
                    return null;

                _clientIP = ipAddress.ToString();
                return _clientIP;
            }
        }
    }

    internal class DatabaseSettings
    {
        public string Name { get; init; } = null!;
        public string ConnectionString { get; init; } = null!;
    }
}
