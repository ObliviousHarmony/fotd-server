#pragma once

#include "ModelSerializer.h"

namespace FOMNetwork {

class AvatarSerializer
    : public ModelSerializer<AvatarSerializer, Avatar> {
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

    bs.WriteBits(&model.face, 5);
    bs.WriteBits(&model.hair, 5);
    bs.WriteBits(&model.faction, 4);
    bs.WriteBits(&model.shirt, 12);
    bs.WriteBits(&model.bottoms, 12);
    bs.WriteBits(&model.shoes, 12);
    bs.WriteBits(&model.gloves, 12);
  }

  void Read(RakNet::BitStream& bs, Avatar& model) const override {
    if (bs.ReadBit())
      model.sex = AvatarGender::Male;
    else
      model.sex = AvatarGender::Female;

    if (bs.ReadBit())
      model.skinColor = AvatarSkin::Light;
    else
      model.skinColor = AvatarSkin::Dark;

      bs.ReadBits(&model.face, 5);
      bs.ReadBits(&model.hair, 5);
      bs.ReadBits(&model.faction, 4);
      bs.ReadBits(&model.shirt, 12);
      bs.ReadBits(&model.bottoms, 12);
      bs.ReadBits(&model.shoes, 12);
      bs.ReadBits(&model.gloves, 12);
  }
};

}  // namespace FOMNetwork
