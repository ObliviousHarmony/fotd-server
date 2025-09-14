using FOMServer.Shared.Models;

namespace FOMServer.Shared.Services
{
	/// <summary>
	/// Interface for logging service.
	/// </summary>
	public interface ILogService
	{
		void Write(LogEntry entry);
	}
}
