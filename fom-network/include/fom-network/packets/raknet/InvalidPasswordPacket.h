#pragma once

#include <fom-network/Interop.h>

namespace FOMNetwork {

#pragma pack(push, 1)
struct InvalidPasswordPacket {};
#pragma pack(pop)

ASSERT_BLITTABLE(InvalidPasswordPacket);

}  // namespace FOMNetwork
