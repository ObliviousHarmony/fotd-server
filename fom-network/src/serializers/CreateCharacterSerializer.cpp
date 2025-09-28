#include <fom-network/PacketSerializers.h>

FOMPacket::CreateCharacter CreateCharacterSerializer::ReadData(
    RakNet::BitStream& bs) const {
  FOMPacket::CreateCharacter data{};
  return data;
}
