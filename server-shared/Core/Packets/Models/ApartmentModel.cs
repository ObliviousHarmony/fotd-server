using System.Runtime.InteropServices;

namespace FOMServer.Shared.Core.Packets.Models
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ApartmentModel
    {
        public uint ID;
        public byte Type;
        public byte World;
    }
}
