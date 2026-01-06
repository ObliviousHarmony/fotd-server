#include <fom-network/packets/RegisterWorld.h>

#include "../types/NetworkAddressSerializer.h"
#include "PacketSerializers.h"

namespace FOMNetwork {

bool RegisterWorldSerializer::Read(RakNet::BitStream& bs,
                                   Packet::RegisterWorld* data) const {
  NetworkAddressSerializer addressSerializer;

  if (!bs.ReadCompressed(data->worldID)) return false;
  if (!addressSerializer.Read(bs, data->clientAddress)) return false;

  return true;
}

void RegisterWorldSerializer::Write(RakNet::BitStream& bs,
                                    const Packet::RegisterWorld* data) const {
  NetworkAddressSerializer addressSerializer;

  bs.WriteCompressed(data->worldID);
  addressSerializer.Write(bs, data->clientAddress);
}

}  // namespace FOMNetwork
