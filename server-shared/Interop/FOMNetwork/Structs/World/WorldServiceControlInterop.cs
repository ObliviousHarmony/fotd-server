using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FOMServer.Shared.Interop.FOMNetwork.Structs;

namespace FOMServer.Shared.Interop.FOMNetwork.Structs.World
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct WorldServiceControlInterop
    {
        public const int MaxPlacements = 32; // MAX_WORLD_SERVICE_CONTROL_PLACEMENTS
        public const int TargetSize = 65;

        public uint ServiceId;
        public byte Type;
        public byte State;
        public byte Security;
        public fixed byte RawTarget[TargetSize];
        public uint NumPlacements;
        public PlacementIdArray PlacementIds;
        public PlacementArray Placements;

        public string Target
        {
            get
            {
                fixed (byte* ptr = RawTarget)
                {
                    return CStringParser.ToString(ptr, TargetSize);
                }
            }
            set
            {
                fixed (byte* ptr = RawTarget)
                {
                    CStringParser.FromString(value, ptr, TargetSize);
                }
            }
        }

        [InlineArray(MaxPlacements)]
        public struct PlacementIdArray
        {
            private uint _id;
        }

        [InlineArray(MaxPlacements)]
        public struct PlacementArray
        {
            private PositionRotationInterop _placement;
        }
    }
}
