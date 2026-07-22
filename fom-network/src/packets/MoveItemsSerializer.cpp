#include <fom-network/packets/MoveItems.h>

#include "PacketSerializers.h"

namespace FOMNetwork {
namespace Packet {

bool MoveItemsSerializer::Read(RakNet::BitStream& bs,
                               Packet::MoveItems* data) const {
  if (!bs.ReadCompressed(data->playerId)) return false;

  if (!bs.ReadCompressed(data->numItemIds)) return false;
  if (data->numItemIds > BufferSizes::MAX_ITEM_LIST_SIZE) return false;
  for (int i = 0; i < data->numItemIds; ++i) {
    if (!bs.ReadCompressed(data->itemIds[i])) return false;
  }

  if (!bs.ReadCompressed(data->from)) return false;
  if (!bs.ReadCompressed(data->to)) return false;
  if (!bs.ReadCompressed(data->fromSlot)) return false;
  if (!bs.ReadCompressed(data->toSlot)) return false;

  return true;
}

void MoveItemsSerializer::Write(RakNet::BitStream& bs,
                                const Packet::MoveItems* data) const {
  auto numItemIds = data->numItemIds;
  if (numItemIds > BufferSizes::MAX_ITEM_LIST_SIZE)
    numItemIds = BufferSizes::MAX_ITEM_LIST_SIZE;

  bs.WriteCompressed(data->playerId);

  bs.WriteCompressed(numItemIds);
  for (int i = 0; i < numItemIds; ++i) {
    bs.WriteCompressed(data->itemIds[i]);
  }

  bs.WriteCompressed(data->from);
  bs.WriteCompressed(data->to);
  bs.WriteCompressed(data->fromSlot);
  bs.WriteCompressed(data->toSlot);
}

}  // namespace Packet
}  // namespace FOMNetwork
