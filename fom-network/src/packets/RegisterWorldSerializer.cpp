#include <fom-network/packets/RegisterWorld.h>

#include "../types/NetworkAddressSerializer.h"
#include "PacketSerializers.h"

namespace FOMNetwork {

bool RegisterWorldSerializer::Read(RakNet::BitStream& bs,
                                   Packet::RegisterWorld* data) const {
  NetworkAddressSerializer addressSerializer;

  if (!addressSerializer.Read(bs, data->publicAddress)) return false;
  if (!bs.ReadCompressed(data->worldIdCount)) return false;
  if (data->worldIdCount > Enum::NUM_WORLDS) return false;
  for (int i = 0; i < data->worldIdCount; ++i) {
    if (!bs.ReadCompressed(data->worldIds[i])) return false;
  }

  return true;
}

void RegisterWorldSerializer::Write(RakNet::BitStream& bs,
                                    const Packet::RegisterWorld* data) const {
  NetworkAddressSerializer addressSerializer;

  addressSerializer.Write(bs, data->publicAddress);
  bs.WriteCompressed(data->worldIdCount);
  for (int i = 0; i < data->worldIdCount; ++i)
    bs.WriteCompressed(data->worldIds[i]);
}

}  // namespace FOMNetwork
