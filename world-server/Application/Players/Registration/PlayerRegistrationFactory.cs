using FOMServer.World.Core.Players;
using FOMServer.World.Core.Players.Registration;

namespace FOMServer.World.Application.Players.Registration
{
    internal class PlayerRegistrationFactory : IPlayerRegistrationFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public PlayerRegistrationFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IPlayerRegistration Create(Player player)
        {
            var registration = ActivatorUtilities.CreateInstance<PlayerRegistration>(_serviceProvider, player);

            registration.Register();

            return registration;
        }
    }
}
