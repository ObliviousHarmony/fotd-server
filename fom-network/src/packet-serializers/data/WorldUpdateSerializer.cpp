#include <fom-network/packets/PacketSerializers.h>

#include <vector>

#include "../models/WorldUpdateModelSerializer.h"

namespace FOMNetwork {

constexpr size_t kMaxCachedStacks = 16384;

void WorldUpdateSerializer::WriteData(RakNet::BitStream& bs,
                                      const Packet::WorldUpdate& data) const {
  WorldUpdateModelSerializer updateSerializer;
  bs.WriteCompressed(data.playerID);
  for (int i = 0; i < data.numUpdates; ++i)
    updateSerializer.Write(bs, data.updates[i]);

  // Write a terminating 0 so the client won't keep
  // trying to read more updates from the packet.
  bs.WriteCompressed((uint8_t)0);
}

}  // namespace FOMNetwork
