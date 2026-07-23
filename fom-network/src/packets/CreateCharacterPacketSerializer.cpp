#include <fom-network/packets/CreateCharacterPacket.h>

#include "../structs/AvatarInteropSerializer.h"
#include "PacketSerializers.h"

namespace FOMNetwork {

bool CreateCharacterPacketSerializer::Read(RakNet::BitStream& bs,
                                           CreateCharacterPacket* data) const {
  AvatarInteropSerializer avatarSerializer;

  if (!bs.ReadCompressed(data->playerId)) return false;
  if (!avatarSerializer.Read(bs, data->avatar)) return false;

  if (!DecodeString(bs, data->name)) return false;
  if (!DecodeString(bs, data->biography)) return false;

  return true;
}

}  // namespace FOMNetwork
