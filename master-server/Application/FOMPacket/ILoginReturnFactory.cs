using FOMServer.Master.Core.Players;
using FOMServer.Shared.Core.FOMPacket.Data;

namespace FOMServer.Master.Application.FOMPacket
{
    public interface ILoginReturnFactory
    {
        LoginReturn Create(Player player);
    }
}
