#include <fom-network/packets/ItemsAdded.h>

#include "../types/ItemListSerializer.h"
#include "PacketSerializers.h"

namespace FOMNetwork {
namespace Packet {

void ItemsAddedSerializer::Write(RakNet::BitStream& bs,
                                 const Packet::ItemsAdded* data) const {
  Type::ItemListSerializer itemListSerializer;

  bs.WriteCompressed(data->playerId);
  bs.WriteCompressed(data->dest);
  itemListSerializer.Write(bs, data->items);
}

}  // namespace Packet
}  // namespace FOMNetwork
