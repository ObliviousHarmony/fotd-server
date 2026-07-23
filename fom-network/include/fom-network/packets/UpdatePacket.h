#pragma once

#include <fom-network/Interop.h>
#include <fom-network/structs/WorldUpdateInterop.h>

namespace FOMNetwork {

#pragma pack(push, 1)
struct UpdatePacket {
  WorldUpdateInterop update;
};
#pragma pack(pop)

ASSERT_BLITTABLE(UpdatePacket);

}  // namespace FOMNetwork
