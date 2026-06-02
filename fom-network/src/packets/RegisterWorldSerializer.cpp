#include <fom-network/packets/RegisterWorld.h>

#include "../types/NetworkAddressSerializer.h"
#include "PacketSerializers.h"

namespace FOMNetwork {
namespace Packet {

bool RegisterWorldSerializer::Read(RakNet::BitStream& bs,
                                   Packet::RegisterWorld* data) const {
  Type::NetworkAddressSerializer addressSerializer;

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
  Type::NetworkAddressSerializer addressSerializer;

  uint8_t worldIdCount = data->worldIdCount;
  if (worldIdCount > Enum::NUM_WORLDS) worldIdCount = Enum::NUM_WORLDS;

  addressSerializer.Write(bs, data->publicAddress);
  bs.WriteCompressed(worldIdCount);
  for (int i = 0; i < worldIdCount; ++i) bs.WriteCompressed(data->worldIds[i]);
}

}  // namespace Packet
}  // namespace FOMNetwork
