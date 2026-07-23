#include <fom-network/packets/CheckNameReturnPacket.h>

#include "PacketSerializers.h"

namespace FOMNetwork {

void CheckNameReturnPacketSerializer::Write(
    RakNet::BitStream& bs, const CheckNameReturnPacket* data) const {
  bs.WriteCompressed(data->ownerPlayerId);
}

}  // namespace FOMNetwork
