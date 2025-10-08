#include <fom-network/packets/PacketSerializers.h>

namespace FOMNetwork {

bool RegisterClientSerializer::ReadData(RakNet::BitStream& bs,
                                        Packet::RegisterClient& data) const {
  bs.ReadCompressed(data.worldID);
  bs.ReadCompressed(data.playerID);
  bs.ReadCompressed(data.worldCRC);
  return true;
}

}  // namespace FOMNetwork
