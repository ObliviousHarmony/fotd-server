#pragma once

#include <fom-network/packets/models/ItemSlotModel.h>

#include "ItemModelSerializer.h"
#include "ModelSerializer.h"

namespace FOMNetwork {

class ItemSlotModelSerializer : public ModelSerializer<Packet::ItemSlotModel> {
 public:
  void Write(RakNet::BitStream& bs,
             const Packet::ItemSlotModel& model) const override {
    ItemModelSerializer itemSerializer;

    if (model.inUse != 0) {
      bs.Write1();
      itemSerializer.Write(bs, model.item);
    } else
      bs.Write0();
  }

  bool Read(RakNet::BitStream& bs,
            Packet::ItemSlotModel& model) const override {
    ItemModelSerializer itemSerializer;

    if (bs.ReadBit()) {
      model.inUse = 1;
      itemSerializer.Read(bs, model.item);
    } else
      model.inUse = 0;

    return true;
  }
};

}  // namespace FOMNetwork
