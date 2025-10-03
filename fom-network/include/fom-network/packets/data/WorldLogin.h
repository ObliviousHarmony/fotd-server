#pragma once

#include <fom-network/Common.h>
#include <fom-network/FOMNetworkEnums.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct WorldLogin {
  WorldID worldID;
  uint8_t nodeID;
  uint32_t playerID;
  uint32_t apartmentID;
};
#pragma pack(pop)

ASSERT_BLITTABLE(WorldLogin);

}  // namespace Packet
}  // namespace FOMNetwork
