using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Constants;

namespace FOMServer.Shared.Core.Packets.Types
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PlayerProfile
    {
        public uint PlayerID;
        public byte Unknown1;
        public PlayerNameBuffer PlayerName;
        public FactionNameBuffer FactionName;
        public BiographyBuffer Biography;
        public RankNameBuffer RankName;

        [InlineArray(BufferSizes.PlayerName)]
        public struct PlayerNameBuffer
        {
            private byte _element;
        }

        [InlineArray(BufferSizes.FactionName)]
        public struct FactionNameBuffer
        {
            private byte _element;
        }

        [InlineArray(BufferSizes.PlayerBiography)]
        public struct BiographyBuffer
        {
            private byte _element;
        }

        [InlineArray(BufferSizes.RankName)]
        public struct RankNameBuffer
        {
            private byte _element;
        }
    }
}
