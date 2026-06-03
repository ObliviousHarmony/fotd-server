#include "../types/WorldUpdateSerializer.h"

#include <fom-network/packets/WorldUpdate.h>

#include "PacketSerializers.h"

namespace FOMNetwork {
namespace Packet {

void WorldUpdateSerializer::Write(RakNet::BitStream& bs,
                                  const Packet::WorldUpdate* data) const {
  Type::WorldUpdateSerializer worldUpdateSerializer;

  uint16_t updateCount = data->updateCount;
  if (updateCount > MAX_WORLD_UPDATES) updateCount = MAX_WORLD_UPDATES;

  bs.WriteCompressed(data->playerId);
  bs.WriteCompressed(data->unknown1);
  for (uint16_t i = 0; i < updateCount; ++i)
    worldUpdateSerializer.Write(bs, data->updates[i]);
}

}  // namespace Packet
}  // namespace FOMNetwork
