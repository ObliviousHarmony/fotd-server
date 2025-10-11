#include <fom-network/packets/PacketSerializers.h>

#include "../models/PlayerUpdateModelSerializer.h"

namespace FOMNetwork {

bool UpdateSerializer::ReadData(RakNet::BitStream& bs,
                                Packet::Update& data) const {
  bs.ReadCompressed(data.type);
  bs.ReadCompressed(data.grid1);
  bs.ReadCompressed(data.grid2);
  bs.ReadCompressed(data.visibilityAreaID);

  switch (data.type) {
    case Enums::WORLDUPDATE_PLAYER: {
      PlayerUpdateModelSerializer playerSerializer;

      auto& updateData = data.data.player;

      updateData.isTurretTargeted = bs.ReadBit() ? 1 : 0;
      if (updateData.isTurretTargeted) bs.ReadCompressed(updateData.turretID);

      updateData.usingMedicalTerminal = bs.ReadBit() ? 1 : 0;
      if (updateData.usingMedicalTerminal)
        bs.ReadCompressed(updateData.treatmentType);

      updateData.isEnemyOfGD = bs.ReadBit() ? 1 : 0;

      playerSerializer.Read(bs, updateData.update);
      break;
    }
  }

  return true;
}

}  // namespace FOMNetwork
