using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket;

namespace FOMServer.Shared.Core.Logging
{
    /// <summary>
    /// Interface for logging service.
    /// </summary>
    public interface ILogService
    {
        void Write(in LogEntry entry);
        void WriteMessage(LogLevel level, string message);
        void WriteException(Exception ex);
        void WritePacketException(Packet packet, Exception ex);
    }
}
