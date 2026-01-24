#pragma once

#include <fom-network/Interop.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct RegisterClient {
  uint8_t worldID;
  uint32_t playerID;
  uint32_t worldCRC;
};
#pragma pack(pop)

ASSERT_BLITTABLE(RegisterClient);

}  // namespace Packet
}  // namespace FOMNetwork
