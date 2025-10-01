using FOMServer.Shared.Core.FOMPacket.Data;

namespace FOMServer.Master.Application.FOMPacket
{
    public interface ILoginReturnFactory
    {
        LoginReturn Create();
    }
}
