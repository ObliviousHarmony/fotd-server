using FOMServer.Shared.Application.Networking;
using FOMServer.Shared.Core.Enums;

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
