#include <fom-network/packets/WorldLoginReturn.h>

#include "../types/NetworkAddressSerializer.h"
#include "PacketSerializers.h"

namespace FOMNetwork {
namespace Packet {

void WorldLoginReturnSerializer::Write(
    RakNet::BitStream& bs, const Packet::WorldLoginReturn* data) const {
  Type::NetworkAddressSerializer addressSerializer;

  bs.WriteCompressed(data->status);
  bs.WriteCompressed(data->worldId);
  addressSerializer.Write(bs, data->worldServerAddress);
}

}  // namespace Packet
}  // namespace FOMNetwork
