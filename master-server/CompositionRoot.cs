using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using FOMServer.Shared.Extensions;
using FOMServer.Master.Handlers;
using FOMServer.Shared.Services.Packets;
using MasterServer.Models;

namespace FOMServer.Master
{
	internal static class CompositionRoot
	{
		public static IServiceProvider BuildContainer()
		{
			ServiceCollection services = new ServiceCollection();

			services.AddServerShared();

			AddConfiguration(services);
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

		private static ServiceCollection AddConfiguration(this ServiceCollection services)
		{
			IConfigurationRoot config = new ConfigurationBuilder()
				.SetBasePath(AppContext.BaseDirectory)
				.AddJsonFile("appsettings.json", optional: false)
				.AddJsonFile("appsettings.Development.json", optional: true)
				.AddEnvironmentVariables()
				.Build();

			ServerSettings serverSettings = config.GetSection("Server").Get<ServerSettings>()!;
			DatabaseSettings dbSettings = config.GetSection("Database").Get<DatabaseSettings>()!;

			if (serverSettings.Port <= 0)
				throw new InvalidOperationException("Server port must be greater than 0.");
			if (string.IsNullOrWhiteSpace(dbSettings.Name))
				throw new InvalidOperationException("Database name must be configured.");
			if (string.IsNullOrWhiteSpace(dbSettings.ConnectionString))
				throw new InvalidOperationException("Database connection string must be configured.");

			services.AddSingleton(serverSettings);
			services.AddSingleton(dbSettings);
			return services;
		}
	}
}
