#include <fom-network/packets/WorldLoginReturn.h>

#include "../types/NetworkAddressSerializer.h"
#include "PacketSerializers.h"

namespace FOMNetwork {

void WorldLoginReturnSerializer::Write(
    RakNet::BitStream& bs, const Packet::WorldLoginReturn* data) const {
  NetworkAddressSerializer addressSerializer;

  bs.WriteCompressed(data->status);
  bs.WriteCompressed(data->worldID);
  addressSerializer.Write(bs, data->worldServerAddress);
}

}  // namespace FOMNetwork
