#pragma once

#include <fom-network/enums/PlayerAttribute.h>
#include <fom-network/packets/models/PlayerAttributesModel.h>

#include "ModelSerializer.h"

namespace FOMNetwork {

class PlayerAttributesModelSerializer
    : public ModelSerializer<Packet::PlayerAttributesModel> {
 public:
  void Write(RakNet::BitStream& bs,
             const Packet::PlayerAttributesModel& model) const override {
    for (int i = 0; i < Enums::NUM_ATTRIBUTES; ++i) {
      bs.WriteCompressed(model.attributes[i]);
    }
  }

  bool Read(RakNet::BitStream& bs,
            Packet::PlayerAttributesModel& model) const override {
    for (int i = 0; i < Enums::NUM_ATTRIBUTES; ++i) {
      bs.ReadCompressed(model.attributes[i]);
    }
    return true;
  }
};

}  // namespace FOMNetwork
