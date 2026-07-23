#pragma once

#include <fom-network/Interop.h>

namespace FOMNetwork {

#pragma pack(push, 1)
struct ConnectionRequestAcceptedPacket {};
#pragma pack(pop)

ASSERT_BLITTABLE(ConnectionRequestAcceptedPacket);

}  // namespace FOMNetwork
