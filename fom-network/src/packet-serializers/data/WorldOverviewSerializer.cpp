#include <fom-network/packets/PacketSerializers.h>

namespace FOMNetwork {

Packet::WorldOverview WorldOverviewSerializer::ReadData(RakNet::BitStream& bs) const {
  Packet::WorldOverview data{};
  bs.Read(data.playerID);
  return data;
}

}  // namespace FOMNetwork
