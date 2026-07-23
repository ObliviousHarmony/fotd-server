using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FOMServer.Shared.Interop.FOMNetwork.Enums;

namespace FOMServer.Shared.Interop.FOMNetwork.Structs.Player
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PlayerSkillsInterop
    {
        public byte TrainingMultiplier;
        public byte CombatTrainingMultiplier;
        public byte EcoTrainingMultiplier;
        public byte TechTrainingMultiplier;
        public uint Unknown1;
        public uint Count;
        public SkillsArray Skills;

        [InlineArray((int)SkillType.NUM_SKILL_TYPES)]
        public struct SkillsArray
        {
            private PlayerSkillInterop _element;
        }
    }
}
