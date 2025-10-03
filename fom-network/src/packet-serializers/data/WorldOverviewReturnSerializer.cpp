#include <fom-network/packets/PacketSerializers.h>

#include <vector>

#include "../models/WorldOverviewModelSerializer.h"

namespace FOMNetwork {

void WorldOverviewReturnSerializer::WriteData(
    RakNet::BitStream& bs, const Packet::WorldOverviewReturn& data) const {
  WorldOverviewModelSerializer overviewSerializer;

  bs.WriteCompressed(data.playerID);
  overviewSerializer.Write(bs, data.worldOverview);
}

}  // namespace FOMNetwork
