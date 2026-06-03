#pragma once

#include <fom-network/Interop.h>
#include <fom-network/types/WorldUpdate.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct Update {
  Type::WorldUpdate update;
};
#pragma pack(pop)

ASSERT_BLITTABLE(Update);

}  // namespace Packet
}  // namespace FOMNetwork
