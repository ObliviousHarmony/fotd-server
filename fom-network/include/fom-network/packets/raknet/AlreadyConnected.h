#pragma once

#include <fom-network/Interop.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct AlreadyConnected {};
#pragma pack(pop)

ASSERT_BLITTABLE(AlreadyConnected);

}  // namespace Packet
}  // namespace FOMNetwork
