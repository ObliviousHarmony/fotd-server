#pragma once

#include <fom-network/Interop.h>

namespace FOMNetwork {

#pragma pack(push, 1)
struct ConnectionBannedPacket {};
#pragma pack(pop)

ASSERT_BLITTABLE(ConnectionBannedPacket);

}  // namespace FOMNetwork
