#pragma once

#include <fom-network/Interop.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct InvalidPassword {};
#pragma pack(pop)

ASSERT_BLITTABLE(InvalidPassword);

}  // namespace Packet
}  // namespace FOMNetwork
