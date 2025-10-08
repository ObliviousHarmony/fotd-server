#include <fom-network/packets/PacketSerializers.h>

#include "../models/AvatarModelSerializer.h"

namespace FOMNetwork {

bool CreateCharacterSerializer::ReadData(RakNet::BitStream& bs,
                                         Packet::CreateCharacter& data) const {
  AvatarModelSerializer avatarSerializer;

  bs.ReadCompressed(data.playerID);
  bs.IgnoreBits(1);
  avatarSerializer.Read(bs, data.avatar);
  DecodeString(bs, data.name);
  DecodeString(bs, data.biography);

  return true;
}

}  // namespace FOMNetwork
