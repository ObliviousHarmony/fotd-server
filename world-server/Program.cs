using FOMServer.World;
using FOMServer.World.Application;

await using var serviceProvider = CompositionRoot.BuildContainer();

var server = serviceProvider.GetRequiredService<Server>();
await server.Run();
