#include <fom-network/packets/PacketSerializers.h>

namespace FOMNetwork {

Packet::WorldLogin WorldLoginSerializer::ReadData(RakNet::BitStream& bs) const {
  Packet::WorldLogin data{};
  bs.ReadCompressed(data.worldID);
  bs.ReadCompressed(data.selectedNodeID);
  bs.ReadCompressed(data.playerID);
  bs.IgnoreBytes(4);  // hardcoded value?
  if (data.worldID == 4) {
    bs.ReadCompressed(data.apartmentID);
  }
  return data;
}

}  // namespace FOMNetwork
