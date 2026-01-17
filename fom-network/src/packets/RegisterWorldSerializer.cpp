#include <fom-network/packets/RegisterWorld.h>

#include "../types/NetworkAddressSerializer.h"
#include "PacketSerializers.h"

namespace FOMNetwork {

bool RegisterWorldSerializer::Read(RakNet::BitStream& bs,
                                   Packet::RegisterWorld* data) const {
  NetworkAddressSerializer addressSerializer;
  if (!addressSerializer.Read(bs, data->clientAddress)) return false;

  if (!bs.ReadCompressed(data->numWorlds)) return false;
  for (int i = 0; i < data->numWorlds; ++i) {
    if (!bs.ReadCompressed(data->worldIDs[i])) return false;
  }

  return true;
}

void RegisterWorldSerializer::Write(RakNet::BitStream& bs,
                                    const Packet::RegisterWorld* data) const {
  NetworkAddressSerializer addressSerializer;
  addressSerializer.Write(bs, data->clientAddress);

  bs.WriteCompressed(data->numWorlds);
  for (int i = 0; i < data->numWorlds; ++i)
    bs.WriteCompressed(data->worldIDs[i]);
}

}  // namespace FOMNetwork
