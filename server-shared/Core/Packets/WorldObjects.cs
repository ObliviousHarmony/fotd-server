using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Packets.Types.World;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Packets
{
    [PacketId(PacketIdentifier.ID_WORLD_OBJECTS)]
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct WorldObjects
    {
        public const int MaxWorldObjects = 512; // MAX_WORLD_OBJECTS
        public const int MaxWorldServices = 32; // MAX_WORLD_SERVICES
        public const int MaxWorldServiceControls = 32; // MAX_WORLD_SERVICE_CONTROLS

        [FieldOffset(0)]
        public WorldObjectsAction Action;

        [FieldOffset(1)]
        public WorldObjectType Type;

        [FieldOffset(3)]
        public uint ObjectId;

        [FieldOffset(7)]
        public byte IsActive;

        [FieldOffset(8)]
        public byte State;

        [FieldOffset(9)]
        public WorldSync Sync;

        [FieldOffset(9)]
        public WorldObject Object;

        [FieldOffset(9)]
        public WorldService Service;

        [FieldOffset(9)]
        public WorldServiceControl Control;

        public enum WorldObjectsAction : byte
        {
            Invalid = 0, // WORLD_OBJECTS_ACTION_INVALID
            WorldSync = 1, // WORLD_OBJECTS_ACTION_WORLD_SYNC
            ObjectSync = 2, // WORLD_OBJECTS_ACTION_OBJECT_SYNC
            SetActive = 3, // WORLD_OBJECTS_ACTION_SET_ACTIVE
            ChangeState = 4, // WORLD_OBJECTS_ACTION_CHANGE_STATE
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct WorldSync
        {
            public uint NumBackpacks;
            public WorldObjectArray Backpacks;
            public uint NumItems;
            public WorldObjectArray Items;
            public uint NumServices;
            public WorldServiceArray Services;
            public uint NumDeployables;
            public WorldObjectArray Deployables;
            public uint NumInfluenceGenerators;
            public WorldObjectArray InfluenceGenerators;
            public uint NumMedicalUnits;
            public WorldObjectArray MedicalUnits;
            public uint NumMiningRigs;
            public WorldObjectArray MiningRigs;
            public uint NumServiceControls;
            public WorldServiceControlArray ServiceControls;
            public uint NumTerritoryObjects;
            public WorldObjectArray TerritoryObjects;
            public uint NumExplosives;
            public WorldObjectArray Explosives;

            [InlineArray(MaxWorldObjects)]
            public struct WorldObjectArray
            {
                private WorldObject _element;
            }

            [InlineArray(MaxWorldServices)]
            public struct WorldServiceArray
            {
                private WorldService _element;
            }

            [InlineArray(MaxWorldServiceControls)]
            public struct WorldServiceControlArray
            {
                private WorldServiceControl _element;
            }
        }
    }
}
