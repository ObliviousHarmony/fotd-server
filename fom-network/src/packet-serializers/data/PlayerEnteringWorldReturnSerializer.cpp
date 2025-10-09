#include <fom-network/packets/PacketSerializers.h>

namespace FOMNetwork {

bool PlayerEnteringWorldReturnSerializer::ReadData(
    RakNet::BitStream& bs, Packet::PlayerEnteringWorldReturn& data) const {
  bs.ReadCompressed(data.status);
  bs.ReadCompressed(data.playerID);

  printf("PlayerEnteringWorldReturnSerializer::Read - PlayerID: %u - Status: %u\n", data.playerID,
         data.status);

  return true;
}

void PlayerEnteringWorldReturnSerializer::WriteData(
    RakNet::BitStream& bs,
    const Packet::PlayerEnteringWorldReturn& data) const {
  bs.WriteCompressed(data.status);
  bs.WriteCompressed(data.playerID);

  printf("PlayerEnteringWorldReturnSerializer::Write - PlayerID: %u - Status: %u\n", data.playerID, data.status);
}

}  // namespace FOMNetwork
