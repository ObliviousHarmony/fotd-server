#include <fom-network/PacketSerializers.h>

namespace FOMNetwork {

Packet::CreateCharacter CreateCharacterSerializer::ReadData(
    RakNet::BitStream& bs) const {
  Packet::CreateCharacter data{};
  return data;
}

}  // namespace FOMNetwork
