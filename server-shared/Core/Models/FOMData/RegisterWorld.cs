using FOMServer.Shared.Core.Enums;
using System.Runtime.InteropServices;

namespace FOMServer.Shared.Core.Models.FOMData
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RegisterWorld
    {
        public WorldID WorldID;
    }
}
