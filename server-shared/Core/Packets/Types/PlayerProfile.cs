using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Constants;

namespace FOMServer.Shared.Core.Packets.Types
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct PlayerProfile
    {
        public uint PlayerId;
        public byte Unknown1;
        public fixed byte RawPlayerName[BufferSizes.PlayerName];
        public fixed byte RawFactionName[BufferSizes.FactionName];
        public fixed byte RawBiography[BufferSizes.PlayerBiography];
        public fixed byte RawRankName[BufferSizes.RankName];

        public string PlayerName
        {
            get
            {
                fixed (byte* ptr = RawPlayerName)
                {
                    return CStringParser.ToString(ptr, BufferSizes.PlayerName);
                }
            }
            set
            {
                fixed (byte* ptr = RawPlayerName)
                {
                    CStringParser.FromString(value, ptr, BufferSizes.PlayerName);
                }
            }
        }

        public string FactionName
        {
            get
            {
                fixed (byte* ptr = RawFactionName)
                {
                    return CStringParser.ToString(ptr, BufferSizes.FactionName);
                }
            }
            set
            {
                fixed (byte* ptr = RawFactionName)
                {
                    CStringParser.FromString(value, ptr, BufferSizes.FactionName);
                }
            }
        }

        public string Biography
        {
            get
            {
                fixed (byte* ptr = RawBiography)
                {
                    return CStringParser.ToString(ptr, BufferSizes.PlayerBiography);
                }
            }
            set
            {
                fixed (byte* ptr = RawBiography)
                {
                    CStringParser.FromString(value, ptr, BufferSizes.PlayerBiography);
                }
            }
        }

        public string RankName
        {
            get
            {
                fixed (byte* ptr = RawRankName)
                {
                    return CStringParser.ToString(ptr, BufferSizes.RankName);
                }
            }
            set
            {
                fixed (byte* ptr = RawRankName)
                {
                    CStringParser.FromString(value, ptr, BufferSizes.RankName);
                }
            }
        }
    }
}
