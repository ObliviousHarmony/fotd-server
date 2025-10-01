using FOMServer.Shared.Core.FOMPacket.Models;

namespace FOMServer.Master.Core.Accounts
{
    public class Account
    {
        public NetworkAddress ClientAddress { get; init; }
        public uint ID { get; init; }
        public string Username { get; init; } = "";
    }
}
