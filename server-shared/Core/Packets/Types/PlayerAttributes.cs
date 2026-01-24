using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;

namespace FOMServer.Shared.Core.Packets.Types
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PlayerAttributes
    {
        public ValuesArray Values;

        [InlineArray((int)AttributeType.NUM_ATTRIBUTE_TYPES)]
        public struct ValuesArray
        {
            private uint _element;
        }
    }
}
