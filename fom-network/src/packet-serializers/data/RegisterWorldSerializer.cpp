#include <fom-network/packets/PacketSerializers.h>

namespace FOMNetwork {

bool RegisterWorldSerializer::ReadData(RakNet::BitStream& bs,
                                       Packet::RegisterWorld& data) const {
  bs.ReadCompressed(data.worldID);
  ReadRawString(bs, data.clientAddress);
  bs.ReadCompressed(data.clientPort);

  return true;
}

void RegisterWorldSerializer::WriteData(
    RakNet::BitStream& bs, const Packet::RegisterWorld& data) const {
  bs.WriteCompressed(data.worldID);
  WriteRawString(bs, data.clientAddress);
  bs.WriteCompressed(data.clientPort);
}

}  // namespace FOMNetwork
