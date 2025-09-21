using FOMServer.Master;
using FOMServer.Master.Application;
using Microsoft.Extensions.DependencyInjection;

class Program
{
	static void Main(string[] args)
	{
		IServiceProvider serviceProvider = CompositionRoot.BuildContainer();

		Server server = serviceProvider.GetRequiredService<Server>();
		server.Run();
	}
}
