#include <fom-network/packets/PacketSerializers.h>

namespace FOMNetwork {

bool PlayerEnteringWorldReturnSerializer::ReadData(
    RakNet::BitStream& bs, Packet::PlayerEnteringWorldReturn& data) const {
  bs.ReadCompressed(data.status);
  bs.ReadCompressed(data.playerID);
  return true;
}

void PlayerEnteringWorldReturnSerializer::WriteData(
    RakNet::BitStream& bs,
    const Packet::PlayerEnteringWorldReturn& data) const {
  bs.WriteCompressed(data.status);
  bs.WriteCompressed(data.playerID);
}

}  // namespace FOMNetwork
