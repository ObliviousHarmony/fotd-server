using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Packets.Models;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Packets.Data
{
    [PacketID(PacketIdentifier.ID_UPDATE)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct Update
    {
        /// <summary>
        /// Native code is using a discriminated union to simplify the
        /// structure. We can replicate this using an explicit layout.
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Pack = 1)]
        public struct UpdateData
        {
            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            public struct PlayerUpdate
            {
                public byte RawIsTurretTargeted;
                public uint TurretID; // RawIsTurretTargeted == 1

                public byte RawUsingMedicalTerminal;
                public MedicalTreatment TreatmentType; // RawUsingMedicalTerminal == 1

                public byte RawIsEnemyOfGD;

                public PlayerUpdateModel Update;
            }

            [FieldOffset(0)] public PlayerUpdate Player;
        }

        public WorldUpdateType Type;
        public uint Grid1;
        public uint Grid2;
        public byte VisibilityAreaID;
        public UpdateData Data;
    }
}
