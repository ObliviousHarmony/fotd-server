#include <fom-network/packets/ItemsChanged.h>

#include "../types/ItemSerializer.h"
#include "PacketSerializers.h"

namespace FOMNetwork {
namespace Packet {

// Matches the client's ID_ITEMS_CHANGED read (CShell.dll FUN_10190990):
// compressed playerId, an 8-bit compressed count, then that many `Item`s.
void ItemsChangedSerializer::Write(RakNet::BitStream& bs,
                                   const Packet::ItemsChanged* data) const {
  uint8_t count = data->count;
  if (count > MAX_ITEMS_CHANGED) count = MAX_ITEMS_CHANGED;

  bs.WriteCompressed(data->playerId);
  bs.WriteCompressed(count);

  Type::ItemSerializer itemSerializer;
  for (uint8_t i = 0; i < count; ++i) itemSerializer.Write(bs, data->items[i]);
}

}  // namespace Packet
}  // namespace FOMNetwork
