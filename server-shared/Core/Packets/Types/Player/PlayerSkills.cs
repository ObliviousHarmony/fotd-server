using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;

namespace FOMServer.Shared.Core.Packets.Types.Player
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PlayerSkills
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
            private PlayerSkill _element;
        }
    }
}
