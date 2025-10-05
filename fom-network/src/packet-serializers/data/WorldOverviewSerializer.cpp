#include <fom-network/packets/PacketSerializers.h>

namespace FOMNetwork {

bool WorldOverviewSerializer::ReadData(RakNet::BitStream& bs,
                                       Packet::WorldOverview& data) const {
  bs.ReadCompressed(data.playerID);

  return true;
}

}  // namespace FOMNetwork
