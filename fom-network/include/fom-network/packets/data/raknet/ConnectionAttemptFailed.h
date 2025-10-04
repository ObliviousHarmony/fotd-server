#pragma once

#include <fom-network/Common.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct ConnectionAttemptFailed {};
#pragma pack(pop)

ASSERT_BLITTABLE(ConnectionAttemptFailed);

}  // namespace Packet
}  // namespace FOMNetwork
