using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Constants;
using FOMServer.Shared.Interop.FOMNetwork.Enums;
using FOMServer.Shared.Interop.FOMNetwork.Structs;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Interop.FOMNetwork.Packets
{
    [PacketId(PacketIdentifier.ID_CREATE_CHARACTER)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct CreateCharacterPacket
    {
        public uint PlayerId;
        public AvatarInterop Avatar;
        public fixed byte RawName[BufferSizes.PlayerName];
        public fixed byte RawBiography[BufferSizes.PlayerBiography];

        public string Name
        {
            get
            {
                fixed (byte* ptr = RawName)
                {
                    return CStringParser.ToString(ptr, BufferSizes.PlayerName);
                }
            }
        }

        public string Biography
        {
            get
            {
                fixed (byte* ptr = RawBiography)
                {
                    return CStringParser.ToString(ptr, BufferSizes.PlayerBiography);
                }
            }
        }
    }
}
