namespace FOMServer.Shared.Core.DTOs
{
    public record AccountDTO
    {
        public uint id { get; init; }
        public string username { get; init; } = "";
        public string password { get; init; } = "";
        public bool logged_in { get; init; }
        public DateTime created_at { get; init; }
        public DateTime updated_at { get; init; }

    }
}
