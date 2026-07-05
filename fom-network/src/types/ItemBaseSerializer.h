#pragma once

#include <fom-network/types/ItemBase.h>

#include "TypeSerializer.h"

namespace FOMNetwork {
namespace Type {

class ItemBaseSerializer : protected TypeSerializer<Type::ItemBase> {
 public:
  void Write(RakNet::BitStream& bs, const Type::ItemBase& data) const {
    bs.WriteCompressed(data.type);
    bs.WriteCompressed(data.value);
    bs.WriteCompressed(data.maxDurability);
    bs.WriteCompressed(data.durability);
    bs.WriteCompressed(data.durabilityLossFactor);
    bs.WriteCompressed(data.security);
    bs.WriteCompressed(data.creatorPlayerId);
    bs.WriteCompressed(data.timeout);
    bs.WriteCompressed(data.stolenFromPlayerId);
    bs.WriteCompressed(data.classification);
    bs.WriteCompressed(data.quality);
    bs.WriteCompressed(data.attributeBonus);

    for (int i = 0; i < BufferSizes::NUM_ITEM_BALANCE_SLIDERS; ++i)
      bs.WriteCompressed(data.balanceValues[i]);
  }

  bool Read(RakNet::BitStream& bs, Type::ItemBase& data) const {
    if (!bs.ReadCompressed(data.type)) return false;
    if (!bs.ReadCompressed(data.value)) return false;
    if (!bs.ReadCompressed(data.maxDurability)) return false;
    if (!bs.ReadCompressed(data.durability)) return false;
    if (!bs.ReadCompressed(data.durabilityLossFactor)) return false;
    if (!bs.ReadCompressed(data.security)) return false;
    if (!bs.ReadCompressed(data.creatorPlayerId)) return false;
    if (!bs.ReadCompressed(data.timeout)) return false;
    if (!bs.ReadCompressed(data.stolenFromPlayerId)) return false;
    if (!bs.ReadCompressed(data.classification)) return false;
    if (!bs.ReadCompressed(data.quality)) return false;
    if (!bs.ReadCompressed(data.attributeBonus)) return false;

    for (int i = 0; i < BufferSizes::NUM_ITEM_BALANCE_SLIDERS; ++i)
      if (!bs.ReadCompressed(data.balanceValues[i])) return false;

    return true;
  }
};

}  // namespace Type
}  // namespace FOMNetwork
