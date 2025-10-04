#pragma once

#include <fom-network/Common.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct DisconnectionNotification {};
#pragma pack(pop)

ASSERT_BLITTABLE(DisconnectionNotification);

}  // namespace Packet
}  // namespace FOMNetwork
