using System.Runtime.InteropServices;

namespace FOMServer.Shared.Interop.FOMNetwork.Structs.Player
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PlayerSkillInterop
    {
        public uint Id;
        public byte Level;
        public uint TrainingTime;
        public byte IsTraining;
        public byte Unknown1;
        public byte Unknown2;
    }
}
