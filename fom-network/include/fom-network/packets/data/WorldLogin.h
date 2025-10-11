#pragma once

#include <fom-network/Common.h>
#include <fom-network/FOMNetworkTypes.h>
#include <fom-network/enums/WorldID.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct WorldLogin {
  Enums::WorldID worldID;
  uint8_t selectedNodeID;
  PlayerID_t playerID;
  uint32_t apartmentID;  // worldID == Enums::APARTMENTS
};
#pragma pack(pop)

ASSERT_BLITTABLE(WorldLogin);

}  // namespace Packet
}  // namespace FOMNetwork
