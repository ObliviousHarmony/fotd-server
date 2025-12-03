using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Networking;

namespace FOMServer.Shared.Core.Logging
{
    public interface ILogService
    {
        void Write(in LogEntry entry);
        void WriteMessage(LogLevel level, string message);
        void WriteException(Exception ex);
        void WritePacketException(in PacketRef packet, Exception ex);
    }
}
