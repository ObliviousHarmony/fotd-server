#pragma once

#include <fom-network/Interop.h>
#include <fom-network/enums/WorldID.h>
#include <fom-network/types/NetworkAddress.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct RegisterWorld {
  Type::NetworkAddress publicAddress;
  uint8_t worldIDCount;
  Enum::WorldID worldIDs[Enum::NUM_WORLDS];
};
#pragma pack(pop)

ASSERT_BLITTABLE(RegisterWorld);

}  // namespace Packet
}  // namespace FOMNetwork
