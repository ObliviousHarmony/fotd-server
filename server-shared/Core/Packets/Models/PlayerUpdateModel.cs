using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;

namespace FOMServer.Shared.Core.Packets.Models
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct PlayerUpdateModel
    {
        public uint PlayerID;
        public PositionRotationModel PositionRotation;
        public AvatarModel Avatar;

        public byte RawIsDead;

        // ------------------ RawIsDead == 0 ------------------
        public sbyte VerticalLookAngle;
        public ushort AnimationID;
        public byte MovementStateID;

        public ItemType EquippedWeapon;
        public byte RawIsWeaponAimed;   // EquippedWeapon != 0
        public byte ConsumedAmmo;       // EquippedWeapon != 0
        public PositionModel FiredFrom; // ConsumedAmmo > 0

        public byte RawWasHit;
        public byte HitAnimationID; // RawWasHit == 1
        public byte HitDirection;   // RawWasHit == 1

        public byte EmoteID;

        public fixed byte RawIsAttachmentEquipped[(int)PlayerAttachment.NUM_ATTACHMENTS];
        public PlayerAttachment ActiveAttachment; // RawIsAttachmentEquipped[n] != 0
        public byte ShieldSetting; // ActiveAttachment == PlayerAttachment.ShieldImplant

        public PlayerUpdateModel()
        {
            // Default Animation (standing idle)
            AnimationID = 16;
        }
    }
}
