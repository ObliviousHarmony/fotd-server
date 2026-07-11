using FOMServer.World.Application.Players.Registration;

namespace FOMServer.World.Core.Players.Registration
{
    internal interface IPlayerRegistrationFactory
    {
        IPlayerRegistration Create(Player player);
    }
}
