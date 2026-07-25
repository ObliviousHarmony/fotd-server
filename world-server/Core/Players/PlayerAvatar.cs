using FOMServer.Shared.Interop.FOMNetwork.Constants;

namespace FOMServer.World.Core.Players
{
    internal class PlayerAvatar
    {
        public AvatarConstants.Sex Sex { get; set; }

        public AvatarConstants.Race Race { get; set; }

        public ushort Face { get; set; }

        public ushort Hair { get; set; }

        public ushort FactionId { get; set; }

        public ushort RankId { get; set; }

        public ushort LegacyFactionId { get; set; }

        public ushort Shirt { get; set; }

        public ushort Bottoms { get; set; }

        public ushort Shoes { get; set; }

        public ushort Hat { get; set; }

        public ushort Head { get; set; }

        public ushort Eyes { get; set; }

        public ushort Shoulder { get; set; }

        public ushort Arms { get; set; }

        public ushort Torso { get; set; }

        public ushort Back { get; set; }

        public ushort Legs { get; set; }

        public ushort Hands { get; set; }

        public bool IsCommander { get; set; }

        public bool IsGroupLeader { get; set; }
    }
}
