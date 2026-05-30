using System.Runtime.InteropServices;

namespace FOMServer.Shared.Core.Packets.Types
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PlayerSkill
    {
        public uint Id;
        public byte Level;
        public uint TrainingTime;
        public byte IsTraining;
        public byte Unknown1;
        public byte Unknown2;
    }
}
