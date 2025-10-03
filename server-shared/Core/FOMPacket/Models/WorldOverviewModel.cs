using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;

namespace FOMServer.Shared.Core.FOMPacket.Models
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WorldOverviewModel
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Entry
        {
            [InlineArray((int)WorldID.NUM_WORLDS)]
            public struct Buffer
            {
                public Entry OverviewWorld;
            }

            public WorldID ID;
            public NetworkAddress Address;
            public ushort PlayerCount;
            public Faction ControllingFaction;
            public FactionRelation ControllingFactionRelation;
        }

        public byte NumWorlds;
        public Entry.Buffer WorldBuffer;
        public uint OnlinePlayers;
        public uint OnlineNewPlayers;
        public byte RawIsPrisoner;
        public ApartmentModel DefaultApartment;

        public bool IsPrisoner
        {
            get => RawIsPrisoner != 0;
            set => RawIsPrisoner = (byte)(value ? 1 : 0);
        }
    }
}
