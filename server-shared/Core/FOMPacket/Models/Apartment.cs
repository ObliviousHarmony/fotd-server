using System.Runtime.InteropServices;

namespace FOMServer.Shared.Core.FOMPacket.Models
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Apartment
    {
        public uint ID;
        public byte Type;
        public byte World;
    }
}
