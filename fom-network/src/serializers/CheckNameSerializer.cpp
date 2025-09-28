#include <fom-network/PacketSerializers.h>

FOMPacket::CheckName CheckNameSerializer::ReadData(
    RakNet::BitStream& bs) const {
  FOMPacket::CheckName data{};
  DecodeString(bs, data.name);
  return data;
}
