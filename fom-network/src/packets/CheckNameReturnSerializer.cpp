#include <fom-network/packets/CheckNameReturn.h>

#include "PacketSerializers.h"

namespace FOMNetwork {
namespace Packet {

void CheckNameReturnSerializer::Write(
    RakNet::BitStream& bs, const Packet::CheckNameReturn* data) const {
  bs.WriteCompressed(data->ownerPlayerId);
}

}  // namespace Packet
}  // namespace FOMNetwork
