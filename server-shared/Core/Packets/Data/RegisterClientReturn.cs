using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Packets.Models;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Packets.Data
{
    [PacketID(PacketIdentifier.ID_REGISTER_CLIENT_RETURN)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct RegisterClientReturn
    {
        public const int NameSize = 20;

        public enum StatusCode : byte
        {
            REGISTER_CLIENT_RETURN_INVALID = 0,
            REGISTER_CLIENT_RETURN_SUCCESS = 1,
            REGISTER_CLIENT_RETURN_ERROR = 2,
            REGISTER_CLIENT_RETURN_WORLD_FULL = 4,
            REGISTER_CLIENT_RETURN_INTEGRITY_CHECK_FAILED = 5,
        };

        public WorldID WorldID;
        public uint PlayerID;
        public StatusCode Status;
        public AvatarModel Avatar;
        public PlayerAttributesModel Attributes;
        public fixed byte RawName[NameSize];
        public byte SelectedNode;

        public string Name
        {
            get
            {
                fixed (byte* ptr = RawName)
                    return CStringParser.ToString(ptr, NameSize);
            }
            set
            {
                fixed (byte* ptr = RawName)
                    CStringParser.FromString(value, ptr, NameSize);
            }
        }
    }
}
