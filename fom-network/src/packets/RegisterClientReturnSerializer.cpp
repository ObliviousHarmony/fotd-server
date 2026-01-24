#include <fom-network/packets/RegisterClientReturn.h>

#include "PacketSerializers.h"

namespace FOMNetwork {

void RegisterClientReturnSerializer::Write(
    RakNet::BitStream& bs, const Packet::RegisterClientReturn* data) const {
  bs.WriteCompressed(data->worldID);
  bs.WriteCompressed(data->playerID);
  bs.WriteCompressed(data->status);
}

}  // namespace FOMNetwork
