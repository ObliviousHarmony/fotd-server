#include <fom-network/PacketSerializers.h>

#include <vector>

#include "../model-serializers/ApartmentSerializer.h"
#include "../model-serializers/OverviewWorldSerializer.h"

namespace FOMNetwork {

void LoginReturnSerializer::WriteData(RakNet::BitStream& bs,
                                      const Packet::LoginReturn& data) const {
  auto worldSerializer = OverviewWorldSerializer::GetInstance();
  auto apartmentSerializer = ApartmentSerializer::GetInstance();

  bs.WriteCompressed(data.status);
  if (data.status != FOMNetwork::Packet::LOGIN_RETURN_SUCCESS &&
      data.status != FOMNetwork::Packet::LOGIN_RETURN_TEMP_BANNED)
    return;

  bs.WriteCompressed(data.playerID);
  bs.WriteCompressed(data.accountType);
  bs.Write(data.isVolunteer == 1);
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
  bs.Write0();  // Unknown Flag
  bs.WriteCompressed(data.onlineNewPlayers);
  apartmentSerializer.Write(bs, data.defaultApartment);
  bs.WriteCompressed((uint8_t)0);   // Unknown Iteration Count
  bs.WriteCompressed((uint32_t)0);  // Show Faction MOTD Prompt
}

}  // namespace FOMNetwork
