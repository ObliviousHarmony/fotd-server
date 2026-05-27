using System.Net;
using System.Net.Sockets;
using FOMServer.Shared.Core.Enums;

namespace FOMServer.World.Core
{
    internal class ServerSettings
    {
        public WorldId[] WorldIds { get; init; } = [];

        public string MasterServerHost { get; init; } = null!;

        public string ClientHost { get; init; } = null!;

        public string? ClientIp
        {
            get
            {
                if (field is not null)
                {
                    return field;
                }

                var hostAddresses = Dns.GetHostAddresses(ClientHost, AddressFamily.InterNetwork);
                if (hostAddresses is null)
                {
                    return null;
                }

                var ipAddress = hostAddresses.FirstOrDefault();
                if (ipAddress is null)
                {
                    return null;
                }

                field = ipAddress.ToString();
                return field;
            }
        }
    }

    internal class DatabaseSettings
    {
        public string Name { get; init; } = null!;

        public string ConnectionString { get; init; } = null!;
    }
}
