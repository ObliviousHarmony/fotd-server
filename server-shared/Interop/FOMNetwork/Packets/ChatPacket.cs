using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Constants;
using FOMServer.Shared.Interop.FOMNetwork.Enums;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Interop.FOMNetwork.Packets
{
    [PacketId(PacketIdentifier.ID_CHAT)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct ChatPacket
    {
        public const int MessageSize = 400;

        public ChatChannel Channel;
        public uint SenderId;
        public uint TargetId; // Channel == CHAT_PRIVATE || CHAT_TRADE || CHAT_GM
        public byte ChatStyle; // Channel != CHAT_SYSTEM
        public fixed byte RawSenderName[BufferSizes.PlayerName];
        public fixed byte RawMessage[MessageSize];

        public string SenderName
        {
            get
            {
                fixed (byte* ptr = RawSenderName)
                {
                    return CStringParser.ToString(ptr, BufferSizes.PlayerName);
                }
            }
            set
            {
                fixed (byte* ptr = RawSenderName)
                {
                    CStringParser.FromString(value, ptr, BufferSizes.Username);
                }
            }
        }

        public string Message
        {
            get
            {
                fixed (byte* ptr = RawMessage)
                {
                    return CStringParser.ToString(ptr, MessageSize);
                }
            }
            set
            {
                fixed (byte* ptr = RawMessage)
                {
                    CStringParser.FromString(value, ptr, MessageSize);
                }
            }
        }
    }
}
