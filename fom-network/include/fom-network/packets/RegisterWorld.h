#pragma once

#include <fom-network/Interop.h>
#include <fom-network/enums/WorldID.h>
#include <fom-network/types/NetworkAddress.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct RegisterWorld {
  Enum::WorldID worldID;
  Type::NetworkAddress clientAddress;
};
#pragma pack(pop)

ASSERT_BLITTABLE(RegisterWorld);

}  // namespace Packet
}  // namespace FOMNetwork
