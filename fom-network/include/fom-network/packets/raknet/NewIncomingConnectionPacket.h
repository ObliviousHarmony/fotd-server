#pragma once

#include <fom-network/Interop.h>

namespace FOMNetwork {

#pragma pack(push, 1)
struct NewIncomingConnectionPacket {};
#pragma pack(pop)

ASSERT_BLITTABLE(NewIncomingConnectionPacket);

}  // namespace FOMNetwork
