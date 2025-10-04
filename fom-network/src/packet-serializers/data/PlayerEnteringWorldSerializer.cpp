#include <fom-network/packets/PacketSerializers.h>

namespace FOMNetwork {

Packet::PlayerEnteringWorld PlayerEnteringWorldSerializer::ReadData(
    RakNet::BitStream& bs) const {
  Packet::PlayerEnteringWorld data{};
  bs.ReadCompressed(data.playerID);
  bs.ReadCompressed(data.selectedNodeID);
  return data;
}

void PlayerEnteringWorldSerializer::WriteData(
    RakNet::BitStream& bs, const Packet::PlayerEnteringWorld& data) const {
  bs.WriteCompressed(data.playerID);
  bs.WriteCompressed(data.selectedNodeID);
}

}  // namespace FOMNetwork
