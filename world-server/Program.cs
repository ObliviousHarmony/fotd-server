using FOMServer.World;
using FOMServer.World.Application;
using Microsoft.Extensions.DependencyInjection;

class Program
{
    static async Task Main(string[] args)
    {
        IServiceProvider serviceProvider = CompositionRoot.BuildContainer();

        var server = serviceProvider.GetRequiredService<Server>();
        await server.Run();
    }
}
