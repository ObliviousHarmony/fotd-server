#pragma once

#include <fom-network/Interop.h>

namespace FOMNetwork {

#pragma pack(push, 1)
struct ConnectionLostPacket {};
#pragma pack(pop)

ASSERT_BLITTABLE(ConnectionLostPacket);

}  // namespace FOMNetwork
