using FOMServer.Master;
using FOMServer.Master.Application;

class Program
{
    static async Task Main(string[] args)
    {
        await using var serviceProvider = CompositionRoot.BuildContainer();

        var server = serviceProvider.GetRequiredService<Server>();
        await server.Run();
    }
}
