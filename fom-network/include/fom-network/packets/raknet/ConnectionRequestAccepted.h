#pragma once

#include <fom-network/Interop.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct ConnectionRequestAccepted {};
#pragma pack(pop)

ASSERT_BLITTABLE(ConnectionRequestAccepted);

}  // namespace Packet
}  // namespace FOMNetwork
