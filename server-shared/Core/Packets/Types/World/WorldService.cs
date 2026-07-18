using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FOMServer.Shared.Core.Packets.Types.World
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct WorldService
    {
        public const int MaxPlacements = 32; // MAX_WORLD_SERVICE_PLACEMENTS
        public const int PathBufferSize = 256;

        public uint Id;
        public byte Type;
        public fixed byte RawModelPaths[PathBufferSize];
        public fixed byte RawSkinPaths[PathBufferSize];
        public fixed byte RawRenderStylePaths[PathBufferSize];
        public ushort Scale;
        public byte MoveToFloor;
        public byte IsSolid;
        public uint NumPlacements;
        public PlacementIdArray PlacementIds;
        public PlacementArray Placements;

        [InlineArray(MaxPlacements)]
        public struct PlacementIdArray
        {
            private uint _id;
        }

        [InlineArray(MaxPlacements)]
        public struct PlacementArray
        {
            private PositionRotation _placement;
        }

        public string ModelPaths
        {
            get
            {
                fixed (byte* ptr = RawModelPaths)
                {
                    return CStringParser.ToString(ptr, PathBufferSize);
                }
            }
            set
            {
                fixed (byte* ptr = RawModelPaths)
                {
                    CStringParser.FromString(value, ptr, PathBufferSize);
                }
            }
        }

        public string SkinPaths
        {
            get
            {
                fixed (byte* ptr = RawSkinPaths)
                {
                    return CStringParser.ToString(ptr, PathBufferSize);
                }
            }
            set
            {
                fixed (byte* ptr = RawSkinPaths)
                {
                    CStringParser.FromString(value, ptr, PathBufferSize);
                }
            }
        }

        public string RenderStylePaths
        {
            get
            {
                fixed (byte* ptr = RawRenderStylePaths)
                {
                    return CStringParser.ToString(ptr, PathBufferSize);
                }
            }
            set
            {
                fixed (byte* ptr = RawRenderStylePaths)
                {
                    CStringParser.FromString(value, ptr, PathBufferSize);
                }
            }
        }
    }
}
