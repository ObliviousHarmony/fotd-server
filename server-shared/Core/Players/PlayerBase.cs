using FOMServer.Shared.Core.Packets;

namespace FOMServer.Shared.Core.Players
{
    public abstract class PlayerBase
    {
        private readonly PlayerSession _session;

        protected PlayerBase(PlayerSession session)
        {
            _session = session;
        }

        public uint ID => _session.ID;
        public NetworkAddress ClientAddress => _session.ClientAddress;
    }
}
