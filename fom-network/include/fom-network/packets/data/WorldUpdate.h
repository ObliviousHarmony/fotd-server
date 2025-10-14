#pragma once

#include <fom-network/Common.h>
#include <fom-network/FOMNetworkTypes.h>
#include <fom-network/packets/models/WorldUpdateModel.h>

namespace FOMNetwork {
namespace Packet {

// The game will not process more than 100 updates at a time.
constexpr uint8_t MaxWorldUpdates = 100;

#pragma pack(push, 1)
struct WorldUpdate {
  PlayerID_t playerID;
  uint8_t numUpdates;
  WorldUpdateModel updates[MaxWorldUpdates];
};
#pragma pack(pop)

ASSERT_BLITTABLE(WorldUpdate);

}  // namespace Packet
}  // namespace FOMNetwork
