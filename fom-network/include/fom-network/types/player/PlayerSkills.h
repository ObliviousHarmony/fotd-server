#pragma once

#include <fom-network/Interop.h>
#include <fom-network/enums/SkillType.h>
#include <fom-network/types/player/PlayerSkill.h>

namespace FOMNetwork {
namespace Type {

#pragma pack(push, 1)
struct PlayerSkills {
  uint8_t trainingMultiplier;
  uint8_t combatTrainingMultiplier;
  uint8_t ecoTrainingMultiplier;
  uint8_t techTrainingMultiplier;
  uint32_t unknown1;
  uint32_t count;
  PlayerSkill skills[Enum::NUM_SKILL_TYPES];
};
#pragma pack(pop)

ASSERT_BLITTABLE(PlayerSkills);

}  // namespace Type
}  // namespace FOMNetwork
