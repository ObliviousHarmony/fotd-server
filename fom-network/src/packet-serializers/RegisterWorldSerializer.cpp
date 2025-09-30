#include <fom-network/PacketSerializers.h>

namespace FOMNetwork {

Packet::RegisterWorld RegisterWorldSerializer::ReadData(RakNet::BitStream& bs) const {
  Packet::RegisterWorld data{};
  bs.ReadCompressed(data.worldID);
  return data;
}

void RegisterWorldSerializer::WriteData(RakNet::BitStream& bs, const Packet::RegisterWorld& data) const {
  bs.WriteCompressed(data.worldID);
}

}  // namespace FOMNetwork
