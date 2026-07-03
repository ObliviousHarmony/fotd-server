#include <fom-network/packets/ItemsRemoved.h>

#include "PacketSerializers.h"

namespace FOMNetwork {
namespace Packet {

// Matches the client's ID_ITEMS_REMOVED read (CShell.dll FUN_10192d40 +
// FUN_1023d7b0): compressed playerId, 8-bit dest, a compressed u16 id count, then
// that many compressed u32 instance ids.
void ItemsRemovedSerializer::Write(RakNet::BitStream& bs,
                                   const Packet::ItemsRemoved* data) const {
  uint16_t idCount = data->idCount;
  if (idCount > MAX_ITEMS_REMOVED) idCount = MAX_ITEMS_REMOVED;

  bs.WriteCompressed(data->playerId);
  bs.WriteCompressed(data->dest);
  bs.WriteCompressed(idCount);
  for (uint16_t i = 0; i < idCount; ++i) bs.WriteCompressed(data->ids[i]);
}

}  // namespace Packet
}  // namespace FOMNetwork
