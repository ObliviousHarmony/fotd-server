using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.FOMPacket.Data
{
    [PacketID(PacketIdentifier.ID_REGISTER_WORLD)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct RegisterWorld
    {
        public WorldID WorldID;
        public fixed byte RawAddress[255];
        public ushort Port;

        public string Address
        {
            get
            {
                fixed (byte* ptr = RawAddress)
                    return CStringParser.ToString(ptr, 18);
            }
            set
            {
                fixed (byte* ptr = RawAddress)
                    CStringParser.FromString(value, ptr, 18);
            }
        }
    }
}
