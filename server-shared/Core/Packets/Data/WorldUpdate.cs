using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Packets.Models;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Packets.Data
{
    [PacketID(PacketIdentifier.ID_WORLD_UPDATE)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WorldUpdate
    {
        public const int MaxWorldUpdates = 100;

        /// <summary>
        /// Native code is using a discriminated union to simplify the
        /// structure. We can replicate this using an explicit layout.
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Pack = 1)]
        public struct UpdateData
        {
            [FieldOffset(0)] public WorldUpdateType Type;

            [FieldOffset(1)] public PlayerUpdateModel Player;
            [FieldOffset(1)] public EnemyUpdateModel Enemy;
        }

        [InlineArray(MaxWorldUpdates)]
        public struct Buffer
        {
            public UpdateData Update;
        }

        public uint PlayerID;
        public byte NumUpdates;
        public Buffer Updates;
    }
}
