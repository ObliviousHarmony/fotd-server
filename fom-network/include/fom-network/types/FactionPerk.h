#pragma once

#include <fom-network/Interop.h>

namespace FOMNetwork {
namespace Type {

// Wire format per perk:
// id (uint16_t), level (uint8_t), active (1 bit)
#pragma pack(push, 1)
struct FactionPerk {
  uint16_t id;
  uint8_t level;
  uint8_t active;
};
#pragma pack(pop)

ASSERT_BLITTABLE(FactionPerk);

}  // namespace Type
}  // namespace FOMNetwork
