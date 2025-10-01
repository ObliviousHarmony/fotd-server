#include <fom-network/PacketSerializers.h>

#include "../model-serializers/OverviewWorldSerializer.h"

namespace FOMNetwork {

void LoginReturnSerializer::WriteData(RakNet::BitStream& bs,
                                      const Packet::LoginReturn& data) const {
  bs.WriteCompressed(data.status);
  if (data.status != FOMNetwork::Packet::LOGIN_RETURN_SUCCESS) return;

  auto worldSerializer = OverviewWorldSerializer::GetInstance();

  bs.WriteCompressed(data.accountID);
  bs.WriteCompressed(data.accountType);
  bs.Write0();  // Volunteer Flag
  bs.WriteCompressed(data.clientVersion);
  bs.Write0();                     // Temp Banned Flag
  bs.Write0();                     // Unknown Flag
  bs.WriteCompressed((uint8_t)0);  // Unknown Byte
  bs.WriteCompressed(data.numWorlds);
  for (size_t i = 0; i < data.numWorlds && i < NUM_WORLDS; i++)
    worldSerializer.Write(bs, data.worldBuffer[i]);
  bs.WriteCompressed(data.onlinePlayers);
  bs.Write0();                     // Show Training Grounds
  bs.WriteCompressed((uint8_t)0);  // Unknown Byte
  WriteBits(bs, data.isPrisoner, 1);
  bs.Write0();                     // Unknown Flag
  bs.WriteCompressed((uint8_t)0);  // Online New Players
}

}  // namespace FOMNetwork
