#include <fom-network/packets/PacketSerializers.h>

namespace FOMNetwork {

bool LoginSerializer::ReadData(RakNet::BitStream& bs, Packet::Login& data) const {
  DecodeString(bs, data.username);
  ReadRawString(bs, data.passwordHash);
  bs.Read(data.clientCRC);
  bs.Read(data.cshellCRC);
  bs.Read(data.objectCRC);
  DecodeString(bs, data.macAddress);

  return true;
}

}  // namespace FOMNetwork
