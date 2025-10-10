#pragma once

#include <fom-network/Common.h>
#include <fom-network/FOMNetworkTypes.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct RegisterWorld {
  WorldID worldID;
  FOMNetwork::NetworkAddress clientAddress;
};
#pragma pack(pop)

ASSERT_BLITTABLE(RegisterWorld);

}  // namespace Packet
}  // namespace FOMNetwork
