#pragma once

#include <fom-network/Interop.h>
#include <fom-network/enums/PlayerSkill.h>
#include <fom-network/types/PlayerSkill.h>

namespace FOMNetwork {
namespace Type {

// Wire format:
// trainingMultiplier (uint8_t), combatTrainingMultiplier (uint8_t),
// ecoTrainingMultiplier (uint8_t), techTrainingMultiplier (uint8_t),
// field_0x20 (uint32_t), count (uint32_t), then array of PlayerSkill
#pragma pack(push, 1)
struct PlayerSkills {
  uint8_t trainingMultiplier;
  uint8_t combatTrainingMultiplier;
  uint8_t ecoTrainingMultiplier;
  uint8_t techTrainingMultiplier;
  uint32_t field_0x20;
  uint32_t count;
  PlayerSkill skills[Enum::NUM_PLAYER_SKILLS];
};
#pragma pack(pop)

ASSERT_BLITTABLE(PlayerSkills);

}  // namespace Type
}  // namespace FOMNetwork
