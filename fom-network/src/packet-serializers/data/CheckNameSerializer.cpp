#include <fom-network/packets/PacketSerializers.h>

namespace FOMNetwork {

Packet::CheckName CheckNameSerializer::ReadData(RakNet::BitStream& bs) const {
  Packet::CheckName data{};
  DecodeString(bs, data.name);
  return data;
}

}  // namespace FOMNetwork
