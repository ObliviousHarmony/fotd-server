#pragma once

#include <fom-network/packets/models/AvatarModel.h>

#include "ModelSerializer.h"

namespace FOMNetwork {

class AvatarModelSerializer : public ModelSerializer<Packet::AvatarModel> {
 public:
  void Write(RakNet::BitStream& bs,
             const Packet::AvatarModel& model) const override {
    WriteBits(bs, model.sex, 1);
    WriteBits(bs, model.skinColor, 1);
    WriteBits(bs, model.face, 5);
    WriteBits(bs, model.hair, 5);
    WriteBits(bs, model.faction, 4);
    WriteBits(bs, model.shirt, 12);
    WriteBits(bs, model.bottoms, 12);
    WriteBits(bs, model.shoes, 12);
    WriteBits(bs, model.gloves, 12);

    if (model.showArmor == 1) {
      bs.Write1();
      WriteBits(bs, model.armorHead, 12);
      WriteBits(bs, model.armorGlasses, 12);
      WriteBits(bs, model.armorShoulder, 12);
      WriteBits(bs, model.armorArm, 12);
      WriteBits(bs, model.armorTorso, 12);
      WriteBits(bs, model.armorLeg, 12);
    } else
      bs.Write0();

    WriteBits(bs, model.rank, 3);
    bs.Write1();
    bs.Write1();
    bs.Write1();
    bs.Write1();
    bs.Write1();
    bs.Write1();
  }

  bool Read(RakNet::BitStream& bs, Packet::AvatarModel& model) const override {
    ReadBits(bs, model.sex, 1);
    ReadBits(bs, model.skinColor, 1);
    ReadBits(bs, model.face, 5);
    ReadBits(bs, model.hair, 5);
    ReadBits(bs, model.faction, 4);
    ReadBits(bs, model.shirt, 12);
    ReadBits(bs, model.bottoms, 12);
    ReadBits(bs, model.shoes, 12);
    ReadBits(bs, model.gloves, 12);

    if (bs.ReadBit()) {
      model.showArmor = 1;
      ReadBits(bs, model.armorHead, 12);
      ReadBits(bs, model.armorGlasses, 12);
      ReadBits(bs, model.armorShoulder, 12);
      ReadBits(bs, model.armorArm, 12);
      ReadBits(bs, model.armorTorso, 12);
      ReadBits(bs, model.armorLeg, 12);
    } else
      model.showArmor = 0;

    ReadBits(bs, model.rank, 3);
    bs.ReadBit();
    bs.ReadBit();
    bs.ReadBit();
    bs.ReadBit();
    bs.ReadBit();
    bs.ReadBit();
    return true;
  }
};

}  // namespace FOMNetwork
