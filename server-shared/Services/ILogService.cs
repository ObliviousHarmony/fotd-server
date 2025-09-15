using FOMServer.Shared.Enums;
using FOMServer.Shared.Models;

namespace FOMServer.Shared.Services
{
	/// <summary>
	/// Interface for logging service.
	/// </summary>
	public interface ILogService
	{
		void Write(in LogEntry entry);
		void WriteMessage(LogLevel level, string message);
	}
}
