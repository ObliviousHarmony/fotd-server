#include <fom-network/packets/PacketSerializers.h>

namespace FOMNetwork {

void WorldLoginReturnSerializer::WriteData(
    RakNet::BitStream& bs, const Packet::WorldLoginReturn& data) const {
  bs.WriteCompressed(data.status);
  bs.WriteCompressed(data.worldID);
}

}  // namespace FOMNetwork
