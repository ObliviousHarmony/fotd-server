#pragma once

#include <fom-network/Common.h>
#include <fom-network/FOMNetworkTypes.h>
#include <fom-network/enums/MedicalTreatment.h>
#include <fom-network/enums/WorldUpdateType.h>
#include <fom-network/packets/models/PlayerUpdateModel.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct Update {
  Enums::WorldUpdateType type;
  uint32_t grid1;
  uint32_t grid2;
  uint8_t visibilityAreaID;

  union {
    struct PlayerUpdate {
      uint32_t targetingTurretID;
      Enums::MedicalTreatment activeMedicalTreatment;
      uint8_t isEnemyOfGD;
      PlayerUpdateModel update;
    } player;

  } data;
};
#pragma pack(pop)

ASSERT_BLITTABLE(Update);

}  // namespace Packet
}  // namespace FOMNetwork
