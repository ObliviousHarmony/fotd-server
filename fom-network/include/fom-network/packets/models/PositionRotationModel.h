#pragma once

#include <fom-network/Common.h>
#include <fom-network/FOMNetworkTypes.h>
#include <fom-network/packets/models/PositionModel.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct PositionRotationModel {
  PositionModel position;
  uint16_t rotation;
};
#pragma pack(pop)

ASSERT_BLITTABLE(PositionRotationModel);

}  // namespace Packet
}  // namespace FOMNetwork
