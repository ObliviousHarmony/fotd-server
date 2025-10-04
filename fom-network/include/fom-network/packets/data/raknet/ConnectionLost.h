#pragma once

#include <fom-network/Common.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct ConnectionLost {};
#pragma pack(pop)

ASSERT_BLITTABLE(ConnectionLost);

}  // namespace Packet
}  // namespace FOMNetwork
