#include <fom-network/packets/AvatarChangePacket.h>

#include "../structs/AvatarInteropSerializer.h"
#include "PacketSerializers.h"

namespace FOMNetwork {

void AvatarChangePacketSerializer::Write(RakNet::BitStream& bs,
                                         const AvatarChangePacket* data) const {
  AvatarInteropSerializer avatarSerializer;

  bs.WriteCompressed(data->playerId);
  avatarSerializer.Write(bs, data->avatar);
}

}  // namespace FOMNetwork
