using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;

namespace FOMServer.Shared.Core.Packets.Types
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct Apartment
    {
        public const int OwnerNameSize = 20;
        public const int EntryCodeSize = 8;
        public const int PublicNameSize = 24;
        public const int PublicDescriptionSize = 512;

        public uint ID;
        public ApartmentType Type;
        public uint OwnerPlayerID;
        public uint OwnerFactionID;
        public byte IsOpen;
        public fixed byte RawOwnerName[OwnerNameSize];
        public fixed byte RawEntryCode[EntryCodeSize];
        public ItemList StorageItems;
        public byte IsPublic;
        public uint EntryPrice;
        public fixed byte RawPublicName[PublicNameSize];
        public fixed byte RawPublicDescription[PublicDescriptionSize];
        public byte IsDefault;
        public byte IsFeatured;
        public uint Occupants;

        public string OwnerName
        {
            get
            {
                fixed (byte* ptr = RawOwnerName)
                    return CStringParser.ToString(ptr, OwnerNameSize);
            }
        }

        public string EntryCode
        {
            get
            {
                fixed (byte* ptr = RawEntryCode)
                    return CStringParser.ToString(ptr, EntryCodeSize);
            }
        }

        public string PublicName
        {
            get
            {
                fixed (byte* ptr = RawPublicName)
                    return CStringParser.ToString(ptr, PublicNameSize);
            }
        }

        public string PublicDescription
        {
            get
            {
                fixed (byte* ptr = RawPublicDescription)
                    return CStringParser.ToString(ptr, PublicDescriptionSize);
            }
        }
    }
}
