#include <fom-network/PacketSerializers.h>

#include "../model-serializers/AvatarSerializer.h"

namespace FOMNetwork {

Packet::CreateCharacter CreateCharacterSerializer::ReadData(
    RakNet::BitStream& bs) const {
  AvatarSerializer avatarSerializer;

  Packet::CreateCharacter data{};
  bs.ReadCompressed(data.playerID);
  bs.IgnoreBits(1);
  avatarSerializer.Read(bs, data.avatar);

  bs.IgnoreBits(1);  // Armor Flag
  bs.IgnoreBits(3);  // Faction Rank
  bs.IgnoreBits(1);
  bs.IgnoreBits(1);
  bs.IgnoreBits(1);
  bs.IgnoreBits(1);
  bs.IgnoreBits(1);
  bs.IgnoreBits(1);
  DecodeString(bs, data.name);
  DecodeString(bs, data.biography);
  return data;
}

}  // namespace FOMNetwork
