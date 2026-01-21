#include <fom-network/packets/LoginReturn.h>

#include "PacketSerializers.h"
#include "../types/ApartmentSerializer.h"

namespace FOMNetwork {

void LoginReturnSerializer::Write(RakNet::BitStream &bs,
                                  const Packet::LoginReturn *data) const {
  ApartmentSerializer apartmentSerializer;

  bs.WriteCompressed(data->status);
  bs.WriteCompressed(data->playerID);

  if (data->playerID == 0) return;

  bs.WriteCompressed(data->accountType);
  bs.Write(data->isVolunteer == 1);
  bs.Write(data->isNewPlayer == 1);
  bs.WriteCompressed(data->clientVersion);

  bs.Write(data->isBanned == 1);
  if (data->isBanned == 1) {
    EncodeString(bs, data->banLength);
    EncodeString(bs, data->banReason);
  }

  bs.WriteCompressed(data->processBlacklistCount);
  for (int i = 0; i < data->processBlacklistCount; ++i) {
    bs.WriteCompressed(data->processBlacklist[i]);
  }

  EncodeString(bs, data->factionMOTD);
  apartmentSerializer.Write(bs, data->defaultApartment);
  bs.WriteCompressed(data->defaultApartmentWorldID);
  bs.WriteCompressed(data->loginWorldID);
}

}  // namespace FOMNetwork
