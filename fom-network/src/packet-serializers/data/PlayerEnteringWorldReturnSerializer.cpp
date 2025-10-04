#include <fom-network/packets/PacketSerializers.h>

namespace FOMNetwork {

Packet::PlayerEnteringWorldReturn PlayerEnteringWorldReturnSerializer::ReadData(
    RakNet::BitStream& bs) const {
  Packet::PlayerEnteringWorldReturn data{};
  bs.ReadCompressed(data.status);
  bs.ReadCompressed(data.playerID);
  return data;
}

void PlayerEnteringWorldReturnSerializer::WriteData(
    RakNet::BitStream& bs,
    const Packet::PlayerEnteringWorldReturn& data) const {
  bs.WriteCompressed(data.status);
  bs.WriteCompressed(data.playerID);
}

}  // namespace FOMNetwork
