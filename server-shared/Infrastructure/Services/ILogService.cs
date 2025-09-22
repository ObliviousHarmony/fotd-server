using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Models;

namespace FOMServer.Shared.Infrastructure.Services
{
	/// <summary>
	/// Interface for logging service.
	/// </summary>
	public interface ILogService
	{
		void Write(in LogEntry entry);
		void WriteMessage(LogLevel level, string message);
		void WriteException(Exception ex);
		void WritePacketException(FOMPacket packet, Exception ex);
	}
}
