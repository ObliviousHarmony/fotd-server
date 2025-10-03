#pragma once

#include <fom-network/Common.h>
#include <fom-network/FOMNetworkEnums.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct ApartmentModel {
  uint32_t id;
  uint8_t type;
  uint8_t world;
};
#pragma pack(pop)

ASSERT_BLITTABLE(ApartmentModel);

}  // namespace Packet
}  // namespace FOMNetwork
