#pragma once

#include <fom-network/Interop.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct NoFreeIncomingConnections {};
#pragma pack(pop)

ASSERT_BLITTABLE(NoFreeIncomingConnections);

}  // namespace Packet
}  // namespace FOMNetwork
