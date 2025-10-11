#include <fom-network/packets/PacketSerializers.h>

#include <vector>

#include "../models/PlayerUpdateModelSerializer.h"

namespace FOMNetwork {

void WorldUpdateSerializer::WriteData(RakNet::BitStream& bs,
                                      const Packet::WorldUpdate& data) const {
  PlayerUpdateModelSerializer updateSerializer;
  bs.WriteCompressed(data.playerID);
  bs.WriteCompressed((uint8_t)2);
  updateSerializer.Write(bs, data.update);
}

}  // namespace FOMNetwork
