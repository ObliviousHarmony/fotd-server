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
      uint8_t isTurretTargeted; // Not needed, the turret ID is 0 if it's not argeted
      uint32_t turretID;  // turretTargeted == 1

      uint8_t usingMedicalTerminal; // Not needed, treatment type is 0 if not using
      Enums::MedicalTreatment treatmentType;  // usingMedicalTerminal == 1

      uint8_t isEnemyOfGD;

      PlayerUpdateModel update;
    } player;

  } data;
};
#pragma pack(pop)

ASSERT_BLITTABLE(Update);

}  // namespace Packet
}  // namespace FOMNetwork
