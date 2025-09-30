#include <fom-network/PacketSerializers.h>

namespace FOMNetwork {

Packet::RegisterWorld RegisterWorldSerializer::ReadData(
    RakNet::BitStream& bs) const {
  Packet::RegisterWorld data{};
  bs.ReadCompressed(data.worldID);
  ReadRawString(bs, data.address);
  bs.Read(data.port);
  return data;
}

void RegisterWorldSerializer::WriteData(
    RakNet::BitStream& bs, const Packet::RegisterWorld& data) const {
  bs.WriteCompressed(data.worldID);
  WriteRawString(bs, data.address);
  bs.Write(data.port);
}

}  // namespace FOMNetwork
