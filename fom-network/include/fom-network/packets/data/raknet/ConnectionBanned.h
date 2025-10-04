#pragma once

#include <fom-network/Common.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct ConnectionBanned {};
#pragma pack(pop)

ASSERT_BLITTABLE(ConnectionBanned);

}  // namespace Packet
}  // namespace FOMNetwork
