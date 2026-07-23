#include <fom-network/packets/WorldUpdatePacket.h>

#include "../structs/WorldUpdateInteropSerializer.h"
#include "PacketSerializers.h"

namespace FOMNetwork {

void WorldUpdatePacketSerializer::Write(RakNet::BitStream& bs,
                                        const WorldUpdatePacket* data) const {
  WorldUpdateInteropSerializer worldUpdatePacketSerializer;

  uint16_t updateCount = data->updateCount;
  if (updateCount > MAX_WORLD_UPDATES) updateCount = MAX_WORLD_UPDATES;

  bs.WriteCompressed(data->playerId);
  bs.WriteCompressed(data->unknown1);
  for (uint16_t i = 0; i < updateCount; ++i)
    worldUpdatePacketSerializer.Write(bs, data->updates[i]);
}

}  // namespace FOMNetwork
