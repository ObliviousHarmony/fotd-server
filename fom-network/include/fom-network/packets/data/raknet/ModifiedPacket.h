#pragma once

#include <fom-network/Common.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct ModifiedPacket {};
#pragma pack(pop)

ASSERT_BLITTABLE(ModifiedPacket);

}  // namespace Packet
}  // namespace FOMNetwork
