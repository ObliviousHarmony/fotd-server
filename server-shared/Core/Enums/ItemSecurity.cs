namespace FOMServer.Shared.Core.Enums
{
    public enum ItemSecurity : byte
    {
        Normal = 0, // ITEM_SECURITY_NORMAL
        Secured = 1, // ITEM_SECURITY_SECURED
        Bound = 2, // ITEM_SECURITY_BOUND
        SpecialBound = 3, // ITEM_SECURITY_SPECIAL_BOUND

        NUM_ITEM_SECURITIES = 4,
    }
}
