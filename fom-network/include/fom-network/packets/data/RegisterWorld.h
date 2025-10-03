#pragma once

#include <fom-network/Common.h>
#include <fom-network/FOMNetworkEnums.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct RegisterWorld {
  WorldID worldID;
  uint8_t address[255];
  uint16_t port;
};
#pragma pack(pop)

ASSERT_BLITTABLE(RegisterWorld);

}  // namespace Packet
}  // namespace FOMNetwork
