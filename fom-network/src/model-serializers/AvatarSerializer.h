#pragma once

#include "ModelSerializer.h"

namespace FOMNetwork {

class AvatarSerializer : public ModelSerializer<AvatarSerializer, Avatar> {
 public:
  void Write(RakNet::BitStream& bs, const Avatar& model) const override {
    if (model.sex == AvatarSex::Male)
      bs.Write1();
    else
      bs.Write0();

    if (model.skinColor == AvatarSkin::Light)
      bs.Write1();
    else
      bs.Write0();

    bs.WriteBits((uint8_t*)&model.face, 5);
    bs.WriteBits((uint8_t*)&model.hair, 5);
    bs.WriteBits((uint8_t*)&model.faction, 4);
    bs.WriteBits((uint8_t*)&model.shirt, 12);
    bs.WriteBits((uint8_t*)&model.bottoms, 12);
    bs.WriteBits((uint8_t*)&model.shoes, 12);
    bs.WriteBits((uint8_t*)&model.gloves, 12);
  }

  bool Read(RakNet::BitStream& bs, Avatar& model) const override {
    if (bs.ReadBit())
      model.sex = AvatarSex::Male;
    else
      model.sex = AvatarSex::Female;

    if (bs.ReadBit())
      model.skinColor = AvatarSkin::Light;
    else
      model.skinColor = AvatarSkin::Dark;

    bs.ReadBits((uint8_t*)&model.face, 5);
    bs.ReadBits((uint8_t*)&model.hair, 5);
    bs.ReadBits((uint8_t*)&model.faction, 4);
    bs.ReadBits((uint8_t*)&model.shirt, 12);
    bs.ReadBits((uint8_t*)&model.bottoms, 12);
    bs.ReadBits((uint8_t*)&model.shoes, 12);
    bs.ReadBits((uint8_t*)&model.gloves, 12);
    return true;
  }
};

}  // namespace FOMNetwork
