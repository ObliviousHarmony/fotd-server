#pragma once

#include <fom-network/Common.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct RSAPublicKeyMismatch {};
#pragma pack(pop)

ASSERT_BLITTABLE(RSAPublicKeyMismatch);

}  // namespace Packet
}  // namespace FOMNetwork
