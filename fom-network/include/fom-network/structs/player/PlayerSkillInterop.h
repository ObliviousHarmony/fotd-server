#pragma once

#include <fom-network/Interop.h>

namespace FOMNetwork {

#pragma pack(push, 1)
struct PlayerSkillInterop {
  uint32_t id;
  uint8_t level;
  uint32_t trainingTime;
  uint8_t isTraining;
  uint8_t unknown1;
  uint8_t unknown2;
};
#pragma pack(pop)

ASSERT_BLITTABLE(PlayerSkillInterop);

}  // namespace FOMNetwork
