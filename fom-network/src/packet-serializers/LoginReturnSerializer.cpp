#include <fom-network/PacketSerializers.h>

#include <vector>

#include "../model-serializers/ApartmentSerializer.h"
#include "../model-serializers/WorldOverviewSerializer.h"

namespace FOMNetwork {

void LoginReturnSerializer::WriteData(RakNet::BitStream& bs,
                                      const Packet::LoginReturn& data) const {
  auto overviewSerializer = WorldOverviewSerializer::GetInstance();

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
  overviewSerializer.Write(bs, data.worldOverview);
  bs.WriteCompressed((uint8_t)0);   // Unknown Iteration Count
  bs.WriteCompressed((uint32_t)0);  // Show Faction MOTD Prompt
}

}  // namespace FOMNetwork
