#include <fom-network/packets/MoveItemsPacket.h>

#include "PacketSerializers.h"

namespace FOMNetwork {

bool MoveItemsPacketSerializer::Read(RakNet::BitStream& bs,
                                     MoveItemsPacket* data) const {
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

void MoveItemsPacketSerializer::Write(RakNet::BitStream& bs,
                                      const MoveItemsPacket* data) const {
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

}  // namespace FOMNetwork
