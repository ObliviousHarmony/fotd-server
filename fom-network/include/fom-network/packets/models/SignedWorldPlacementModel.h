#pragma once

#include <fom-network/Common.h>
#include <fom-network/FOMNetworkTypes.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct SignedWorldPlacementModel {
  int16_t x;
  int16_t y;
  int16_t z;
  uint16_t rotation;
};
#pragma pack(pop)

ASSERT_BLITTABLE(SignedWorldPlacementModel);

}  // namespace Packet
}  // namespace FOMNetwork
