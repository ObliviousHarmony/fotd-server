#include <fom-network/PacketSerializers.h>

namespace FOMNetwork {

void CheckNameReturnSerializer::WriteData(
    RakNet::BitStream& bs, const Packet::CheckNameReturn& data) const {
  bs.WriteCompressed(data.existingPlayerID);
}

}  // namespace FOMNetwork
