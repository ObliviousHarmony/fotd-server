#pragma once

#include <fom-network/Common.h>
#include <fom-network/FOMNetworkTypes.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct PositionModel {
  int16_t x;
  int16_t y;
  int16_t z;
};
#pragma pack(pop)

ASSERT_BLITTABLE(PositionModel);

}  // namespace Packet
}  // namespace FOMNetwork
