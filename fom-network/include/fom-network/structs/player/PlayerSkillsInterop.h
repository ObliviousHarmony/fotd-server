#pragma once

#include <fom-network/Interop.h>
#include <fom-network/enums/SkillType.h>
#include <fom-network/structs/player/PlayerSkillInterop.h>

namespace FOMNetwork {

#pragma pack(push, 1)
struct PlayerSkillsInterop {
  uint8_t trainingMultiplier;
  uint8_t combatTrainingMultiplier;
  uint8_t ecoTrainingMultiplier;
  uint8_t techTrainingMultiplier;
  uint32_t unknown1;
  uint32_t count;
  PlayerSkillInterop skills[Enum::NUM_SKILL_TYPES];
};
#pragma pack(pop)

ASSERT_BLITTABLE(PlayerSkillsInterop);

}  // namespace FOMNetwork
