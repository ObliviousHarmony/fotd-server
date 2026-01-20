using System.Data;

namespace FOMServer.Shared.Infrastructure
{
    public interface IDbConnectionFactory
    {
        IDbConnection Create();
    }
}
