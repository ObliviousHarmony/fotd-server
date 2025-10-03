#pragma once

#include <fom-network/packets/models/ApartmentModel.h>

#include "ModelSerializer.h"

namespace FOMNetwork {

class ApartmentModelSerializer
    : public ModelSerializer<Packet::ApartmentModel> {
 public:
  void Write(RakNet::BitStream& bs,
             const Packet::ApartmentModel& model) const override {
    bs.WriteCompressed(model.id);
    bs.WriteCompressed(model.type);
    bs.WriteCompressed(model.world);
    bs.WriteCompressed((uint8_t)0);
  }

  bool Read(RakNet::BitStream& bs,
            Packet::ApartmentModel& model) const override {
    bs.ReadCompressed(model.id);
    bs.ReadCompressed(model.type);
    bs.ReadCompressed(model.world);
    bs.IgnoreBytes(1);
    return true;
  }
};

}  // namespace FOMNetwork
