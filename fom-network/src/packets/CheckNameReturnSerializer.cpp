#include <fom-network/packets/CheckNameReturn.h>

#include "PacketSerializers.h"

namespace FOMNetwork {

void CheckNameReturnSerializer::Write(
    RakNet::BitStream& bs, const Packet::CheckNameReturn* data) const {
  bs.WriteCompressed(data->ownerPlayerID);
}

}  // namespace FOMNetwork
