using System.Runtime.InteropServices;
using FOMServer.Shared.Interop.FOMNetwork.Enums;
using FOMServer.Shared.Interop.FOMNetwork.Structs.Player;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Interop.FOMNetwork.Packets
{
    [PacketId(PacketIdentifier.ID_ATTRIBUTE_CHANGE)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct AttributeChangePacket
    {
        public long ChangedMask;
        public PlayerAttributesInterop RawAttributes;

        public void Set(AttributeType attribute, uint value)
        {
            attribute.ApplyToMask(ref ChangedMask, true);

            fixed (uint* values = RawAttributes.Values)
            {
                values[(int)attribute] = value;
            }
        }
    }
}
