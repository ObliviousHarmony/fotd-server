#include <fom-network/PacketSerializers.h>

FOMPacket::Login LoginSerializer::ReadData(RakNet::BitStream& bs) const {
  FOMPacket::Login data{};
  DecodeString(bs, data.username);
  ReadRawString(bs, data.passwordHash);
  bs.Read(data.clientCRC);
  bs.Read(data.cshellCRC);
  bs.Read(data.objectCRC);
  DecodeString(bs, data.macAddress);
  return data;
}
