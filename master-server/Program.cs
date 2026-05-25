using FOMServer.Master;
using FOMServer.Master.Application;

await using var serviceProvider = CompositionRoot.BuildContainer();

var server = serviceProvider.GetRequiredService<Server>();
await server.Run();
