#pragma once

#include <fom-network/Interop.h>

namespace FOMNetwork {

#pragma pack(push, 1)
struct ModifiedPacketPacket {};
#pragma pack(pop)

ASSERT_BLITTABLE(ModifiedPacketPacket);

}  // namespace FOMNetwork
