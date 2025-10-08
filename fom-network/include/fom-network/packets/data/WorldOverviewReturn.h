#pragma once

#include <fom-network/Common.h>
#include <fom-network/packets/models/WorldOverviewModel.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct WorldOverviewReturn {
  PlayerID_t playerID;
  WorldOverviewModel worldOverview;
};
#pragma pack(pop)

ASSERT_BLITTABLE(WorldOverviewReturn);

}  // namespace Packet
}  // namespace FOMNetwork
