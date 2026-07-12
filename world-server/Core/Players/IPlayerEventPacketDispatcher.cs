using System;
using System.Collections.Generic;
using System.Text;

namespace FOMServer.World.Core.Players
{
    internal interface IPlayerEventPacketDispatcher
    {
        void Register(Player player);

        void Unregister(Player player);
    }
}
