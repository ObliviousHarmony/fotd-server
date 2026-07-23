#pragma once

#include <fom-network/Interop.h>

namespace FOMNetwork {

#pragma pack(push, 1)
struct NoFreeIncomingConnectionsPacket {};
#pragma pack(pop)

ASSERT_BLITTABLE(NoFreeIncomingConnectionsPacket);

}  // namespace FOMNetwork
