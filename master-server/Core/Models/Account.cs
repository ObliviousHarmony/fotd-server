using FOMServer.Shared.Core.Models;

namespace FOMServer.Master.Core.Models
{
    public class Account
    {
        public NetworkAddress ClientAddress { get; init; }
        public uint ID { get; init; }
        public string Username { get; init; } = "";
    }
}
