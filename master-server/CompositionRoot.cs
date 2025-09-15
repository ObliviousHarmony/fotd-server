using Microsoft.Extensions.DependencyInjection;
using FOMServer.Shared.Extensions;
using FOMServer.Shared.Services;
using FOMServer.Master.Services;

namespace FOMServer.Master
{
	internal static class CompositionRoot
	{
		public static IServiceProvider BuildContainer()
		{
			ServiceCollection services = new ServiceCollection();

			services.AddServerShared();

			services.AddSingleton<ServerNetworkManager>();
			services.AddSingleton<ISendPackets, ServerNetworkManager>(sp => sp.GetRequiredService<ServerNetworkManager>());

			services.AddSingleton<Server>();
			return services.BuildServiceProvider();
		}
	}
}
