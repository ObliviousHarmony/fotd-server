#pragma once

#include <fom-network/Interop.h>
#include <fom-network/NetworkAddress.h>
#include <fom-network/enums/WorldId.h>

namespace FOMNetwork {

#pragma pack(push, 1)
struct RegisterWorldPacket {
  NetworkAddress publicAddress;
  uint8_t worldIdCount;
  Enum::WorldId worldIds[Enum::NUM_WORLDS];
};
#pragma pack(pop)

ASSERT_BLITTABLE(RegisterWorldPacket);

}  // namespace FOMNetwork
