#include <fom-network/PacketSerializers.h>

#include <vector>

#include "../model-serializers/OverviewWorldSerializer.h"

namespace FOMNetwork {

void LoginReturnSerializer::WriteData(RakNet::BitStream& bs,
                                      const Packet::LoginReturn& data) const {
  bs.WriteCompressed(data.status);
  if (data.status != FOMNetwork::Packet::LOGIN_RETURN_SUCCESS &&
      data.status != FOMNetwork::Packet::LOGIN_RETURN_TEMP_BANNED)
    return;

  auto worldSerializer = OverviewWorldSerializer::GetInstance();

  bs.WriteCompressed(data.playerID);
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
  bs.Write(data.isPrisoner == 1);
  bs.Write0();                      // Unknown Flag
  bs.WriteCompressed((uint32_t)0);  // Online New Players

  // Default Apartment
  bs.WriteCompressed((uint32_t)0);  // ID
  bs.WriteCompressed((uint8_t)0);   // Type
  bs.WriteCompressed((uint8_t)0);   // World
  bs.WriteCompressed((uint8_t)0);   // ?

  bs.WriteCompressed((uint8_t)0);   // Unknown Iteration Count
  bs.WriteCompressed((uint32_t)0);  // Show Faction MOTD Prompt
}

}  // namespace FOMNetwork
