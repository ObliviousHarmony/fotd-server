#pragma once

#include <fom-network/Interop.h>

namespace FOMNetwork {

#pragma pack(push, 1)
struct RegisterClientPacket {
  uint8_t worldId;
  uint32_t playerId;
  uint32_t worldCrc;
};
#pragma pack(pop)

ASSERT_BLITTABLE(RegisterClientPacket);

}  // namespace FOMNetwork
