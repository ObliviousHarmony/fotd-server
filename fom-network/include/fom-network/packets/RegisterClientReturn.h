#pragma once

#include <fom-network/Interop.h>

namespace FOMNetwork {
namespace Packet {

enum RegisterClientReturnStatus : uint8_t {
  REGISTER_CLIENT_RETURN_SUCCESS = 1,
  REGISTER_CLIENT_RETURN_WORLD_FULL = 4,
  REGISTER_CLIENT_RETURN_INVALID_WORLD_FILE = 5,
};

#pragma pack(push, 1)
struct RegisterClientReturn {
  uint8_t worldID;
  uint32_t playerID;
  RegisterClientReturnStatus status;
};
#pragma pack(pop)

ASSERT_BLITTABLE(RegisterClientReturn);

}  // namespace Packet
}  // namespace FOMNetwork
