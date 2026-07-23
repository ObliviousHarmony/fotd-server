using FOMServer.Shared.Interop.FOMNetwork.Enums.Item;

namespace FOMServer.Shared.Core.Constants
{
    /// <summary>
    /// Inventory-related constants for slot counts and limits.
    /// </summary>
    public static class PlayerConstants
    {
        public const int NumEquipmentSlots = (int)ItemSlotType.NumEquipmentSlots; // NUM_EQUIPMENT_SLOTS
        public const int NumWeaponSlots = (int)ItemSlotType.NumWeaponSlots; // NUM_WEAPON_SLOTS
        public const int NumQuickslots = (int)ItemSlotType.NumQuickslots; // NUM_QUICKSLOTS
        public const int NumActiveConsumableSlots = 3; // NUM_ACTIVE_CONSUMABLE_SLOTS
        public const int NumNanomachineAugmentationSlots = 3; // NUM_NANOMACHINE_AUGMENTATION_SLOTS

        /// <summary>
        /// Default value for each attribute, indexed by type.
        /// </summary>
        public static readonly uint[] AttributeDefaultValues =
        [
            1000, // Health
            0, // Stamina
            0, // BioEnergy
            1000, // Aura
            0, // UniversalCredits
            0, // FactionCredits
            0, // Penalty
            0, // PrisonerStatus
            0, // HighestPenalty
            0, // MostWantedStatus
            0, // WantedStatus
            900, // Agility
            0, // BallisticDamage
            0, // EnergyDamage
            0, // BioDamage
            0, // AuraDamage
            0, // Destruction
            0, // WeaponRecoil
            0, // Armor
            0, // Shielding
            0, // Resistance
            0, // Reflection
            0, // HealthRegeneration
            0, // StaminaRegeneration
            0, // BioRegeneration
            30, // AuraRegeneration
            0, // Coins
            0, // HealingCooldown
            0, // FoodCooldown
            0, // XenoDamage
            0, // HealthDrain
            0, // StaminaDrain
            0, // BioEnergyDrain
            0, // AuraDrain
            0, // ProtectionBypass
            0, // EffectiveRange
            0, // WeaponFireDelay
            0, //
            0, //
            0, // Weight
            1000, // JumpVelocityMultiplier
            1000, // FallDamageMultiplier
            0, // Nightvision
            0, // SoundlessMovement
            125, // ActivationDistance
            1000, // SprintSpeedMultiplier
            4000, // MaxStamina
            0, // BioEnergyReplenishingCooldown
            0, // AuraHealingCooldown
            0, // EmergencyShield
            0, // EmergencyShieldCooldown
            0, // VortexEmitterCountdown
            0, // Unknown
        ];

        /// <summary>
        /// Maximum value for each attribute, indexed by type.
        /// </summary>
        public static readonly uint[] AttributeMaxValues =
        [
            1000, // Health
            10000, // Stamina
            1000, // BioEnergy
            1000, // Aura
            1_000_000_000, // UniversalCredits
            1_000_000_000, // FactionCredits
            1_000_000_000, // Penalty
            32, // PrisonerStatus
            1_000_000_000, // HighestPenalty
            1, // MostWantedStatus
            1, // WantedStatus
            1000, // Agility
            1000, // BallisticDamage
            1000, // EnergyDamage
            1000, // BioDamage
            1000, // AuraDamage
            10000, // Destruction
            3000, // WeaponRecoil
            1000, // Armor
            1000, // Shielding
            1000, // Resistance
            1000, // Reflection
            1000, // HealthRegeneration
            600, // StaminaRegeneration
            1000, // BioRegeneration
            1000, // AuraRegeneration
            1_000_000_000, // Coins
            int.MaxValue, // HealingCooldown
            int.MaxValue, // FoodCooldown
            1000, // XenoDamage
            1000, // HealthDrain
            1000, // StaminaDrain
            1000, // BioEnergyDrain
            1000, // AuraDrain
            1000, // ProtectionBypass
            5000, // EffectiveRange
            2000, // WeaponFireDelay
            1000, //
            1000, //
            500000, // Weight
            2000, // JumpVelocityMultiplier
            1000, // FallDamageMultiplier
            1, // Nightvision
            1, // SoundlessMovement
            500, // ActivationDistance
            2000, // SprintSpeedMultiplier
            10000, // MaxStamina
            int.MaxValue, // BioEnergyReplenishingCooldown
            int.MaxValue, // AuraHealingCooldown
            5, // EmergencyShield
            300, // EmergencyShieldCooldown
            30, // VortexEmitterCountdown
            600, // Unknown
        ];
    }
}
