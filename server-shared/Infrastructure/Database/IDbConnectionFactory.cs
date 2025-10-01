using System.Data;

namespace FOMServer.Shared.Infrastructure.Database
{
    public interface IDbConnectionFactory
    {
        IDbConnection Create();
    }
}
