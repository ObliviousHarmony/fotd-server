#pragma once

#include <fom-network/Common.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct NewIncomingConnection {};
#pragma pack(pop)

ASSERT_BLITTABLE(NewIncomingConnection);

}  // namespace Packet
}  // namespace FOMNetwork
