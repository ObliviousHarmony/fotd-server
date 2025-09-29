using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Models;

namespace FOMServer.Shared.Infrastructure.Services
{
    /// <summary>
    /// Interface for managing the logging service.
    /// </summary>
    public interface IManageLogService
    {
        /// <summary>
        /// Starts the logging service.
        /// </summary>
        void Start(CancellationToken ctParent);

        /// <summary>
        /// Stops the logging service.
        /// </summary>
        Task StopAsync();
    }
}
