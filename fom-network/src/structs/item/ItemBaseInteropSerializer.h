#pragma once

#include <fom-network/constants/ItemConstants.h>
#include <fom-network/structs/item/ItemBaseInterop.h>

#include "../InteropTypeSerializer.h"

namespace FOMNetwork {

class ItemBaseInteropSerializer
    : protected InteropTypeSerializer<ItemBaseInterop> {
 public:
  void Write(RakNet::BitStream& bs, const ItemBaseInterop& data) const {
    bs.WriteCompressed(data.type);
    bs.WriteCompressed(data.value);
    bs.WriteCompressed(data.durability);
    bs.WriteCompressed(data.valueMax);
    bs.WriteCompressed(data.durabilityLossFactor);
    bs.WriteCompressed(data.security);
    bs.WriteCompressed(data.creatorPlayerId);
    bs.WriteCompressed(data.timeout);
    bs.WriteCompressed(data.stolenFromPlayerId);
    bs.WriteCompressed(data.recipeVariation);
    bs.WriteCompressed(data.rarity);
    bs.WriteCompressed(data.attributeBonus);

    for (int i = 0; i < BufferSizes::NUM_ITEM_BALANCE_SLIDERS; ++i) {
      auto val = data.recipeBalanceValues[i];
      if (val > Constants::ITEM_RECIPE_BALANCE_SLIDER_MAX)
        val = Constants::ITEM_RECIPE_BALANCE_SLIDER_MAX;

      bs.WriteCompressed(val);
    }
  }

  bool Read(RakNet::BitStream& bs, ItemBaseInterop& data) const {
    if (!bs.ReadCompressed(data.type)) return false;
    if (!bs.ReadCompressed(data.value)) return false;
    if (!bs.ReadCompressed(data.durability)) return false;
    if (!bs.ReadCompressed(data.valueMax)) return false;
    if (!bs.ReadCompressed(data.durabilityLossFactor)) return false;
    if (!bs.ReadCompressed(data.security)) return false;
    if (!bs.ReadCompressed(data.creatorPlayerId)) return false;
    if (!bs.ReadCompressed(data.timeout)) return false;
    if (!bs.ReadCompressed(data.stolenFromPlayerId)) return false;
    if (!bs.ReadCompressed(data.recipeVariation)) return false;
    if (!bs.ReadCompressed(data.rarity)) return false;
    if (!bs.ReadCompressed(data.attributeBonus)) return false;

    for (int i = 0; i < BufferSizes::NUM_ITEM_BALANCE_SLIDERS; ++i) {
      if (!bs.ReadCompressed(data.recipeBalanceValues[i])) return false;
      if (data.recipeBalanceValues[i] >
          Constants::ITEM_RECIPE_BALANCE_SLIDER_MAX)
        return false;
    }

    return true;
  }
};

}  // namespace FOMNetwork
