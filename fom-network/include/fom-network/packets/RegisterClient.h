#pragma once

#include <fom-network/Interop.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct RegisterClient {
  uint8_t worldId;
  uint32_t playerId;
  uint32_t worldCrc;
};
#pragma pack(pop)

ASSERT_BLITTABLE(RegisterClient);

}  // namespace Packet
}  // namespace FOMNetwork
