#pragma once

#include <fom-network/Common.h>
#include <fom-network/WorldID.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct RegisterWorld {
  WorldID worldID;
};
#pragma pack(pop)

ASSERT_BLITTABLE(RegisterWorld);

}  // namespace Packet
}  // namespace FOMNetwork
