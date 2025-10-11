#pragma once

#include <fom-network/Common.h>
#include <fom-network/FOMNetworkTypes.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct WorldOverview {
  PlayerID_t playerID;
};
#pragma pack(pop)

ASSERT_BLITTABLE(WorldOverview);

}  // namespace Packet
}  // namespace FOMNetwork
