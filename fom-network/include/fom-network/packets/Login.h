#pragma once

#include <fom-network/Common.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct Login {
  uint8_t username[19];
  uint8_t passwordHash[32];
  uint32_t clientCRC;
  uint32_t cshellCRC;
  uint32_t objectCRC;
  uint8_t macAddress[18];
};
#pragma pack(pop)

ASSERT_BLITTABLE(Login);

}  // namespace Packet
}  // namespace FOMNetwork
