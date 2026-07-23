#include <fom-network/packets/WorldLoginReturnPacket.h>

#include "../NetworkAddressSerializer.h"
#include "PacketSerializers.h"

namespace FOMNetwork {

void WorldLoginReturnPacketSerializer::Write(
    RakNet::BitStream& bs, const WorldLoginReturnPacket* data) const {
  NetworkAddressSerializer addressSerializer;

  bs.WriteCompressed(data->status);
  bs.WriteCompressed(data->worldId);
  addressSerializer.Write(bs, data->worldServerAddress);
}

}  // namespace FOMNetwork
