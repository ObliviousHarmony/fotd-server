#include <fom-network/packets/MoveItems.h>

#include "PacketSerializers.h"

namespace FOMNetwork {
namespace Packet {

bool MoveItemsSerializer::Read(RakNet::BitStream& bs,
                               Packet::MoveItems* data) const {
  if (!bs.ReadCompressed(data->playerId)) return false;

  if (!bs.ReadCompressed(data->numIds)) return false;
  if (data->numIds > BufferSizes::MAX_ITEM_LIST_SIZE) return false;
  for (int i = 0; i < data->numIds; ++i) {
    if (!bs.ReadCompressed(data->ids[i])) return false;
  }

  if (!bs.ReadCompressed(data->source)) return false;
  if (!bs.ReadCompressed(data->destination)) return false;
  if (!bs.ReadCompressed(data->sourceIndex)) return false;
  if (!bs.ReadCompressed(data->destinationIndex)) return false;

  return true;
}

void MoveItemsSerializer::Write(RakNet::BitStream& bs,
                                const Packet::MoveItems* data) const {
  bs.WriteCompressed(data->playerId);

  auto numIds = data->numIds;
  if (numIds > BufferSizes::MAX_ITEM_LIST_SIZE)
    numIds = BufferSizes::MAX_ITEM_LIST_SIZE;
  bs.WriteCompressed(numIds);
  for (int i = 0; i < numIds; ++i) {
    bs.WriteCompressed(data->ids[i]);
  }

  bs.WriteCompressed(data->source);
  bs.WriteCompressed(data->destination);
  bs.WriteCompressed(data->sourceIndex);
  bs.WriteCompressed(data->destinationIndex);
}

}  // namespace Packet
}  // namespace FOMNetwork
