#pragma once

#include <fom-network/Interop.h>

namespace FOMNetwork {
namespace Type {

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
