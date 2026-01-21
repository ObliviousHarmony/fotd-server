#pragma once

#include <fom-network/Interop.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct CheckName {
  uint8_t name[BufferSizes::PLAYER_NAME];
  uint32_t playerID;
};
#pragma pack(pop)

ASSERT_BLITTABLE(CheckName);

}  // namespace Packet
}  // namespace FOMNetwork
