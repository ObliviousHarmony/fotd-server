#include <fom-network/packets/RegisterWorldPacket.h>

#include "../NetworkAddressSerializer.h"
#include "PacketSerializers.h"

namespace FOMNetwork {

bool RegisterWorldPacketSerializer::Read(RakNet::BitStream& bs,
                                         RegisterWorldPacket* data) const {
  NetworkAddressSerializer addressSerializer;

  if (!addressSerializer.Read(bs, data->publicAddress)) return false;
  if (!bs.ReadCompressed(data->worldIdCount)) return false;
  if (data->worldIdCount > Enum::NUM_WORLDS) return false;
  for (int i = 0; i < data->worldIdCount; ++i) {
    if (!bs.ReadCompressed(data->worldIds[i])) return false;
  }

  return true;
}

void RegisterWorldPacketSerializer::Write(
    RakNet::BitStream& bs, const RegisterWorldPacket* data) const {
  NetworkAddressSerializer addressSerializer;

  uint8_t worldIdCount = data->worldIdCount;
  if (worldIdCount > Enum::NUM_WORLDS) worldIdCount = Enum::NUM_WORLDS;

  addressSerializer.Write(bs, data->publicAddress);
  bs.WriteCompressed(worldIdCount);
  for (int i = 0; i < worldIdCount; ++i) bs.WriteCompressed(data->worldIds[i]);
}

}  // namespace FOMNetwork
