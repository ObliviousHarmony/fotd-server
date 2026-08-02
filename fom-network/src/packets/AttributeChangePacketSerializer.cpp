#include <fom-network/packets/AttributeChangePacket.h>

#include "PacketSerializers.h"

namespace FOMNetwork {

void AttributeChangePacketSerializer::Write(
    RakNet::BitStream& bs, const AttributeChangePacket* data) const {
  uint8_t count = 0;
  for (uint8_t i = 0; i < Enum::NUM_ATTRIBUTE_TYPES; ++i) {
    if (data->changedMask & (1LL << i)) ++count;
  }

  bs.WriteCompressed(count);
  for (uint8_t i = 0; i < Enum::NUM_ATTRIBUTE_TYPES; ++i) {
    if (!(data->changedMask & (1LL << i))) continue;

    bs.WriteCompressed(i);
    bs.WriteCompressed(data->attributes.values[i]);
  }
}

}  // namespace FOMNetwork
