#include <fom-network/packets/CreateCharacter.h>

#include "../types/AvatarSerializer.h"
#include "PacketSerializers.h"

namespace FOMNetwork {

bool CreateCharacterSerializer::Read(RakNet::BitStream& bs,
                                     Packet::CreateCharacter* data) const {
  AvatarSerializer avatarSerializer;

  if (!bs.ReadCompressed(data->playerID)) return false;
  if (!avatarSerializer.Read(bs, data->avatar)) return false;
  if (!DecodeString(bs, data->name)) return false;
  if (!DecodeString(bs, data->biography)) return false;

  return true;
}

}  // namespace FOMNetwork
