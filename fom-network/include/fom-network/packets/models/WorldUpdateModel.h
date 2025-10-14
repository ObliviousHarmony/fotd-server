#pragma once
#pragma once

#include <fom-network/Common.h>
#include <fom-network/FOMNetworkTypes.h>
#include <fom-network/enums/WorldUpdateType.h>
#include <fom-network/packets/models/EnemyUpdateModel.h>
#include <fom-network/packets/models/PlayerUpdateModel.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct WorldUpdateModel {
  Enums::WorldUpdateType type;
  union {
    PlayerUpdateModel player;
    EnemyUpdateModel enemy;
  } data;
};
#pragma pack(pop)

ASSERT_BLITTABLE(WorldUpdateModel);

}  // namespace Packet
}  // namespace FOMNetwork
