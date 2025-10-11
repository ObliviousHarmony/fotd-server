#pragma once

#include <fom-network/Common.h>
#include <fom-network/FOMNetworkTypes.h>
#include <fom-network/enums/WorldID.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct RegisterClient {
  Enums::WorldID worldID;
  PlayerID_t playerID;
  uint32_t worldCRC;
};
#pragma pack(pop)

ASSERT_BLITTABLE(RegisterClient);

}  // namespace Packet
}  // namespace FOMNetwork
