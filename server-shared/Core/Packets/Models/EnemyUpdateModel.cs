using System.Runtime.InteropServices;

namespace FOMServer.Shared.Core.Packets.Models
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct EnemyUpdateModel
    {
        public uint ControllingPlayerID;
        public uint EnemyID;
        public ushort EnemyType;
        public SignedWorldPlacementModel Placement;

        public byte StateFlags;

        // ------------------ StateFlags == 0 ------------------

        public byte RawWasHit;
        public byte MovementState;
        public byte AIState;

        public byte RawIsAttacking;
        public byte AttackAnimation; // RawIsAttacking == 1
    }
}
