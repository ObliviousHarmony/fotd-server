#include <fom-network/packets/LoginReturn.h>

#include "../types/ApartmentSerializer.h"
#include "PacketSerializers.h"

namespace FOMNetwork {

void LoginReturnSerializer::Write(RakNet::BitStream &bs,
                                  const Packet::LoginReturn *data) const {
  ApartmentSerializer apartmentSerializer;

  bs.WriteCompressed(data->status);
  bs.WriteCompressed(data->playerId);

  if (data->playerId == 0) return;

  bs.WriteCompressed(data->accountType);
  bs.Write(data->isVolunteer == 1);
  bs.Write(data->isNewPlayer == 1);
  bs.WriteCompressed(data->clientVersion);

  bs.Write(data->isBanned == 1);
  if (data->isBanned == 1) {
    EncodeString(bs, data->banLength);
    EncodeString(bs, data->banReason);
  }

  uint8_t processBlacklistCount = data->processBlacklistCount;
  if (processBlacklistCount > Packet::MAX_PROCESS_BLACKLIST)
    processBlacklistCount = Packet::MAX_PROCESS_BLACKLIST;
  bs.WriteCompressed(processBlacklistCount);
  for (int i = 0; i < processBlacklistCount; ++i) {
    bs.WriteCompressed(data->processBlacklist[i]);
  }

  EncodeString(bs, data->factionMotd);

  apartmentSerializer.Write(bs, data->defaultApartment);
  bs.WriteCompressed(data->defaultApartmentWorldId);

  bs.WriteCompressed(data->loginWorldId);
}

}  // namespace FOMNetwork
