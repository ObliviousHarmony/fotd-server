#pragma once

#include <fom-network/packets/models/ItemModel.h>
#include <fom-network/enums/ItemType.h>

#include "ModelSerializer.h"

namespace FOMNetwork {

class ItemModelSerializer : public ModelSerializer<Packet::ItemModel> {
 public:
  void Write(RakNet::BitStream& bs,
             const Packet::ItemModel& model) const override {
    bs.WriteCompressed(model.id);
    bs.WriteCompressed(model.type);
    bs.WriteCompressed(model.value);
    bs.WriteCompressed(model.durability);
    bs.Write(model.isFactionItem != 0);
  }

  bool Read(RakNet::BitStream& bs, Packet::ItemModel& model) const override {
    bs.ReadCompressed(model.id);
    bs.ReadCompressed(model.type);
    bs.ReadCompressed(model.value);
    bs.ReadCompressed(model.durability);
    model.isFactionItem = bs.ReadBit() ? 1 : 0;
    return true;
  }

  void WriteStacks(RakNet::BitStream& bs, const Packet::ItemModel* items,
                   const int numItems) const;
  bool ReadStacks(RakNet::BitStream& bs, int maxItems, Packet::ItemModel* items,
                  int& numItems) const;

 private:
  inline uint64_t PackItemStackKey(Enums::ItemType type, uint16_t value,
                                   uint16_t durability,
                                   uint8_t isFactionItem) const noexcept {
    uint64_t key = 0;
    key |= ((uint64_t)type & 0xFFFF);
    key |= ((uint64_t)value & 0xFFFF) << 16;
    key |= ((uint64_t)durability & 0xFFFF) << 32;
    key |= ((uint64_t)isFactionItem & 0xFF) << 48;
    return key;
  }

  inline void UnpackItemStackKey(uint64_t key, Enums::ItemType& type, uint16_t& value,
                                 uint16_t& durability,
                                 uint8_t& isFactionItem) const noexcept {
    type = (Enums::ItemType)(key & 0xFFFF);
    value = (uint16_t)((key >> 16) & 0xFFFF);
    durability = (uint16_t)((key >> 32) & 0xFFFF);
    isFactionItem = (uint8_t)((key >> 48) & 0xFF);
  }
};

}  // namespace FOMNetwork
