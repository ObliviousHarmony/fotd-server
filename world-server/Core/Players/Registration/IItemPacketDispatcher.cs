using System;
using System.Collections.Generic;
using System.Text;
using FOMServer.Shared.Core.Items;

namespace FOMServer.World.Core.Players.Registration
{
    internal interface IItemPacketDispatcher
    {
        void Register(IItemLocation location);

        void Unregister(IItemLocation location);
    }
}
