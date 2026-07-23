using System.Runtime.InteropServices;

namespace FOMServer.Shared.Interop.FOMNetwork.Structs
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct WorldUpdateInterop
    {
        [FieldOffset(0)]
        public Type Kind;

        [FieldOffset(1)]
        public PlayerUpdate Player;

        [FieldOffset(1)]
        public CharacterUpdate Character;

        public enum Type : byte
        {
            Invalid = 0, // WORLD_UPDATE_TYPE_INVALID
            Player = 1, // WORLD_UPDATE_TYPE_PLAYER
            Character = 2, // WORLD_UPDATE_TYPE_CHARACTER
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct PlayerUpdate
        {
            public uint Grid1;
            public uint Grid2;
            public byte VisibilityAreaId;
            public uint TargetingTurretId;
            public byte ActiveMedicalTreatment;
            public uint Unknown1;

            public CharacterUpdate Character;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct CharacterUpdate
        {
            public uint Id;
            public PositionRotationInterop Position;
            public AvatarInterop Avatar;
            public byte IsDead;

            // === !isDead ===
            public short VerticalLookAngle;
            public ushort AnimationId;
            public byte MovementStateId;

            public ushort EquippedWeapon;
            public byte IsWeaponAimed;
            public byte ConsumedAmmo;
            public PositionInterop FiredPosition;

            public byte WasHit;
            public byte HitAnimationId;
            public byte HitDirection;

            public byte EmoteId;

            public ushort ActiveImplants;
            public byte ShieldSetting;

            public byte MovementSpeed;
            public byte Unknown2;
            public byte Unknown3;
            public ushort Unknown4;
            public ushort Unknown5;
            public byte IsShieldActive;
        }
    }
}
