using Microsoft.Extensions.DependencyInjection;
using FOMServer.Shared.Extensions;
using FOMServer.Master.Handlers;
using FOMServer.Shared.Services.Packets;

namespace FOMServer.Master
{
	internal static class CompositionRoot
	{
		public static IServiceProvider BuildContainer()
		{
			ServiceCollection services = new ServiceCollection();

			services.AddServerShared();

			AddPacketHandlers(services);

			services.AddSingleton<Server>();
			return services.BuildServiceProvider();
		}

		private static ServiceCollection AddPacketHandlers(this ServiceCollection services)
		{
			services.AddSingleton<IPacketHandler, IncomingConectionHandler>();
			services.AddSingleton<IPacketHandler, LoginRequestHandler>();
			return services;
		}
	}
}
