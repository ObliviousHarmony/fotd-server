namespace FOMServer.Shared.Core.Enums
{
    public enum AccountType : byte
    {
        Invalid = 0, // ACCOUNT_TYPE_INVALID
        Free = 1, // ACCOUNT_TYPE_FREE
        Prepaid = 2, // ACCOUNT_TYPE_PREPAID
        Subscription = 3, // ACCOUNT_TYPE_SUBSCRIPTION

        NUM_ACCOUNT_TYPES, // Unknown
    }
}
