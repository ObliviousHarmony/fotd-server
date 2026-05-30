#pragma once

#include <fom-network/Interop.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct RsaPublicKeyMismatch {};
#pragma pack(pop)

ASSERT_BLITTABLE(RsaPublicKeyMismatch);

}  // namespace Packet
}  // namespace FOMNetwork
