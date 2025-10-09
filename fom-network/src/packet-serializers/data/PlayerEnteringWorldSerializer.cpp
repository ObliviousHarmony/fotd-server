#include <fom-network/packets/PacketSerializers.h>

namespace FOMNetwork {

bool PlayerEnteringWorldSerializer::ReadData(
    RakNet::BitStream& bs, Packet::PlayerEnteringWorld& data) const {
  bs.ReadCompressed(data.playerID);
  bs.ReadCompressed(data.selectedNodeID);

  printf("PlayerEnteringWorldSerializer::Read - PlayerID: %u - Node ID: %u\n", data.playerID, data.selectedNodeID);

  return true;
}

void PlayerEnteringWorldSerializer::WriteData(
    RakNet::BitStream& bs, const Packet::PlayerEnteringWorld& data) const {
  bs.WriteCompressed(data.playerID);
  bs.WriteCompressed(data.selectedNodeID);

  printf("PlayerEnteringWorldSerializer::Write - PlayerID: %u - Node ID: %u\n", data.playerID, data.selectedNodeID);
}

}  // namespace FOMNetwork
