#pragma once

#include <fom-network/Interop.h>

namespace FOMNetwork {

#pragma pack(push, 1)
struct DisconnectionNotificationPacket {};
#pragma pack(pop)

ASSERT_BLITTABLE(DisconnectionNotificationPacket);

}  // namespace FOMNetwork
