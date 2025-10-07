using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Packets.Data
{
    [PacketID(PacketIdentifier.ID_REGISTER_WORLD)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct RegisterWorld
    {
        public const int AddressSize = 255;

        public WorldID WorldID;
        public fixed byte RawClientAddress[AddressSize];
        public ushort ClientPort;

        public string ClientAddress
        {
            get
            {
                fixed (byte* ptr = RawClientAddress)
                    return CStringParser.ToString(ptr, AddressSize);
            }
            set
            {
                fixed (byte* ptr = RawClientAddress)
                    CStringParser.FromString(value, ptr, AddressSize);
            }
        }
    }
}
