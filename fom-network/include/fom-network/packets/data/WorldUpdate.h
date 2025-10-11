#pragma once

#include <fom-network/Common.h>
#include <fom-network/FOMNetworkTypes.h>
#include <fom-network/packets/models/PlayerUpdateModel.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct WorldUpdate {
  PlayerID_t playerID;
  PlayerUpdateModel update;
};
#pragma pack(pop)

ASSERT_BLITTABLE(WorldUpdate);

}  // namespace Packet
}  // namespace FOMNetwork
