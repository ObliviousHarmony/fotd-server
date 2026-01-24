#pragma once

#include <fom-network/Interop.h>

namespace FOMNetwork {
namespace Type {

// Wire format per skill:
// id (uint32_t), level (uint8_t), trainingTime (uint32_t),
// isTraining (uint8_t), unknown1 (uint8_t), unknown2 (uint8_t)
#pragma pack(push, 1)
struct PlayerSkill {
  uint32_t id;
  uint8_t level;
  uint32_t trainingTime;
  uint8_t isTraining;
  uint8_t unknown1;
  uint8_t unknown2;
};
#pragma pack(pop)

ASSERT_BLITTABLE(PlayerSkill);

}  // namespace Type
}  // namespace FOMNetwork
