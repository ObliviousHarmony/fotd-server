namespace FOMServer.Shared.Core.Enums
{
    public enum AttributeType : byte
    {
        Health = 0,
        Stamina = 1,
        BioEnergy = 2,
        Aura = 3,
        UniversalCredits = 4,
        FactionCredits = 5,
        Penalty = 6,
        PrisonerStatus = 7,
        HighestPenalty = 8,
        MostWantedStatus = 9,
        WantedStatus = 10,
        Agility = 11,
        BallisticDamage = 12,
        EnergyDamage = 13,
        BioDamage = 14,
        AuraDamage = 15,
        Destruction = 16,
        WeaponRecoil = 17,
        Armor = 18,
        Shielding = 19,
        Resistance = 20,
        Reflection = 21,
        HealthRegeneration = 22,
        StaminaRegeneration = 23,
        BioRegeneration = 24,
        AuraRegeneration = 25,
        Coins = 26,
        HealingCooldown = 27,
        FoodCooldown = 28,
        XenoDamage = 29,
        HealthDrain = 30,
        StaminaDrain = 31,
        BioEnergyDrain = 32,
        AuraDrain = 33,
        ProtectionBypass = 34,
        EffectiveRange = 35,
        WeaponFireDelay = 36,

        // 37
        // 38

        Weight = 39,
        JumpVelocityMultiplier = 40,
        FallDamageMultiplier = 41,
        Nightvision = 42,
        SoundlessMovement = 43,
        ActivationDistance = 44,
        SprintSpeedMultiplier = 45,
        MaxStamina = 46,
        BioEnergyReplenishingCooldown = 47,
        AuraHealingCooldown = 48,
        EmergencyShield = 49,
        EmergencyShieldCooldown = 50,
        VortexEmitterCountdown = 51,

        // 52

        NUM_ATTRIBUTE_TYPES = 53,
    }

    public static class AttributeTypeExtensions
    {
        public static long ToMask(this AttributeType attribute)
        {
            return 1L << (int)attribute;
        }

        public static void ApplyToMask(this AttributeType attribute, ref long mask, bool isSet)
        {
            if (isSet)
            {
                mask |= 1L << (int)attribute;
            }
            else
            {
                mask &= ~(1L << (int)attribute);
            }
        }

        public static bool IsMaskSet(this AttributeType attribute, long mask)
        {
            return (mask & attribute.ToMask()) != 0;
        }
    }
}
