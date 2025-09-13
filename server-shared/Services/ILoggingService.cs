using FOMServer.Shared.Models;

namespace FOMServer.Shared.Services
{
	/// <summary>
	/// Interface for logging service.
	/// </summary>
	public interface ILoggingService
	{
		void Enqueue(LogEntry entry);
	}
}
