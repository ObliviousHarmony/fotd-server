using System.Data;

namespace FOMServer.Shared.Infrastructure.Factories
{
	public interface IConnectionFactory
	{
		IDbConnection Create();
	}
}
