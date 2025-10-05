#include <fom-network/packets/PacketSerializers.h>

namespace FOMNetwork {

bool CheckNameSerializer::ReadData(RakNet::BitStream& bs,
                                   Packet::CheckName& data) const {
  DecodeString(bs, data.name);

  return true;
}

}  // namespace FOMNetwork
