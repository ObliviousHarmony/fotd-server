#pragma once

#include <fom-network/Interop.h>
#include <fom-network/enums/PlayerAttribute.h>

namespace FOMNetwork {
namespace Type {

// Wire format: 53x uint32_t values
#pragma pack(push, 1)
struct PlayerAttributes {
  uint32_t values[Enum::NUM_PLAYER_ATTRIBUTES];
};
#pragma pack(pop)

ASSERT_BLITTABLE(PlayerAttributes);

}  // namespace Type
}  // namespace FOMNetwork
