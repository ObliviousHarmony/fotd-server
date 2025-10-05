#include <fom-network/packets/PacketSerializers.h>

namespace FOMNetwork {

bool RegisterWorldSerializer::ReadData(RakNet::BitStream& bs,
                                       Packet::RegisterWorld& data) const {
  bs.ReadCompressed(data.worldID);
  ReadRawString(bs, data.address);
  bs.ReadCompressed(data.port);

  return true;
}

void RegisterWorldSerializer::WriteData(
    RakNet::BitStream& bs, const Packet::RegisterWorld& data) const {
  bs.WriteCompressed(data.worldID);
  WriteRawString(bs, data.address);
  bs.WriteCompressed(data.port);
}

}  // namespace FOMNetwork
