using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Constants;
using FOMServer.Shared.Interop.FOMNetwork.Enums;
using FOMServer.Shared.Interop.FOMNetwork.Structs.Item;

namespace FOMServer.Shared.Interop.FOMNetwork.Structs
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct ApartmentInterop
    {
        public const int EntryCodeSize = 8;
        public const int PublicNameSize = 24;
        public const int PublicDescriptionSize = 512;

        public uint Id;
        public ApartmentType Type;
        public uint OwnerPlayerId;
        public uint OwnerFactionId;
        public byte IsOpen;
        public fixed byte RawOwnerName[BufferSizes.PlayerName];
        public fixed byte RawEntryCode[EntryCodeSize];
        public ItemListInterop StorageItems;
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
                {
                    return CStringParser.ToString(ptr, BufferSizes.PlayerName);
                }
            }
            set
            {
                fixed (byte* ptr = RawOwnerName)
                {
                    CStringParser.FromString(value, ptr, BufferSizes.PlayerName);
                }
            }
        }

        public string EntryCode
        {
            get
            {
                fixed (byte* ptr = RawEntryCode)
                {
                    return CStringParser.ToString(ptr, EntryCodeSize);
                }
            }
            set
            {
                fixed (byte* ptr = RawEntryCode)
                {
                    CStringParser.FromString(value, ptr, EntryCodeSize);
                }
            }
        }

        public string PublicName
        {
            get
            {
                fixed (byte* ptr = RawPublicName)
                {
                    return CStringParser.ToString(ptr, PublicNameSize);
                }
            }
            set
            {
                fixed (byte* ptr = RawPublicName)
                {
                    CStringParser.FromString(value, ptr, PublicNameSize);
                }
            }
        }

        public string PublicDescription
        {
            get
            {
                fixed (byte* ptr = RawPublicDescription)
                {
                    return CStringParser.ToString(ptr, PublicDescriptionSize);
                }
            }
            set
            {
                fixed (byte* ptr = RawPublicDescription)
                {
                    CStringParser.FromString(value, ptr, PublicDescriptionSize);
                }
            }
        }
    }
}
