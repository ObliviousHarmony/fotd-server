using System.Data;

namespace FOMServer.Shared.Infrastructure.Factories
{
    public interface IDbConnectionFactory
    {
        IDbConnection Create();
    }
}
