using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;

namespace FOMServer.Shared.Core.Packets.Models
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct PlayerUpdateModel
    {
        public uint PlayerID;
        public WorldPlacementModel Placement;
        public AvatarModel Avatar;

        public byte RawIsDead;

        // ------------------ RawIsDead == 0 ------------------
        public byte VerticalLookAngle;

        public byte RawIsAnimating;
        public ushort AnimationID; // RawIsAnimating == 1

        public byte RawIsMoving;
        public byte MovementStateID; // RawIsMoving == 1

        public byte RawHasWeaponEquipped;
        public ItemType EquippedWeapon; // RawHasWeaponEquipped == 1
        public byte RawIsWeaponAimed;   // RawHasWeaponEquipped == 1
        public byte RawIsWeaponFiring;  // RawHasWeaponEquipped == 1
        public byte CurrentAmmo;        // RawIsWeaponFiring == 1
        public ushort FiredPosX;        // RawIsWeaponFiring == 1
        public ushort FiredPosY;        // RawIsWeaponFiring == 1
        public ushort FiredPosZ;        // RawIsWeaponFiring == 1

        public byte RawWasHit;
        public byte HitAnimationID; // RawWasHit = 1
        public byte HitDirection;   // RawWasHit = 1

        public byte RawIsEmoting;
        public byte EmoteID; // RawIsEmoting = 1

        public byte RawHasAttachments;
        public fixed byte RawIsAttachmentEquipped[(int)PlayerAttachment.NUM_ATTACHMENTS]; // RawHasAttachments == 1
        public PlayerAttachment ActiveAttachment;                                         // RawHasAttachments == 1
        public byte ShieldSetting; // ActiveAttachment == PlayerAttachment.ShieldImplant
    }
}
