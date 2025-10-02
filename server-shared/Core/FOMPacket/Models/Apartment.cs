using FOMServer.Shared.Core.Enums;
using System.Runtime.InteropServices;

namespace FOMServer.Shared.Core.FOMPacket.Models
{
    /// <summary>
    /// Represents an apartment.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Apartment
    {
        public uint ID;
        public byte Type;
        public byte World;
    }
}
