#pragma once

#include <fom-network/Common.h>
#include <fom-network/FOMNetworkTypes.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct WorldPlacementModel {
  uint16_t x;
  uint16_t y;
  uint16_t z;
  uint16_t rotation;
};
#pragma pack(pop)

ASSERT_BLITTABLE(WorldPlacementModel);

}  // namespace Packet
}  // namespace FOMNetwork
