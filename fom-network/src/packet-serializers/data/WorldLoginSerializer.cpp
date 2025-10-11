#include <fom-network/packets/PacketSerializers.h>

namespace FOMNetwork {

bool WorldLoginSerializer::ReadData(RakNet::BitStream& bs,
                                    Packet::WorldLogin& data) const {
  bs.ReadCompressed(data.worldID);
  bs.ReadCompressed(data.selectedNodeID);
  bs.ReadCompressed(data.playerID);
  bs.IgnoreBytes(4);  // hardcoded value?
  if (data.worldID == Enums::APARTMENTS) {
    bs.ReadCompressed(data.apartmentID);
  }

  return true;
}

}  // namespace FOMNetwork
