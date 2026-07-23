#pragma once

#include <fom-network/Interop.h>

namespace FOMNetwork {

#pragma pack(push, 1)
struct RsaPublicKeyMismatchPacket {};
#pragma pack(pop)

ASSERT_BLITTABLE(RsaPublicKeyMismatchPacket);

}  // namespace FOMNetwork
