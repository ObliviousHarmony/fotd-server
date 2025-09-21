using System.Data;

namespace FOMServer.Shared.Factories
{
	public interface IConnectionFactory
	{
		IDbConnection Create();
	}
}
