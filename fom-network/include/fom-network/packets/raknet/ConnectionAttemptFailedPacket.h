#pragma once

#include <fom-network/Interop.h>

namespace FOMNetwork {

#pragma pack(push, 1)
struct ConnectionAttemptFailedPacket {};
#pragma pack(pop)

ASSERT_BLITTABLE(ConnectionAttemptFailedPacket);

}  // namespace FOMNetwork
