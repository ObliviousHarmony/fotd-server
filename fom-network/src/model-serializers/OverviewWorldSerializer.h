#pragma once

#include <fom-network/models/OverviewWorld.h>
#include "NetworkAddressSerializer.h"

#include "ModelSerializer.h"

namespace FOMNetwork {

class OverviewWorldSerializer
    : public ModelSerializer<OverviewWorldSerializer, OverviewWorld> {
 public:
  void Write(RakNet::BitStream& bs, const OverviewWorld& model) const override {
    auto addressSerializer = NetworkAddressSerializer::GetInstance();

    bs.WriteCompressed(model.id);
    addressSerializer.Write(bs, model.address);
    bs.WriteCompressed(model.playerCount);
    bs.WriteCompressed(model.controllingFaction);
    bs.WriteCompressed(model.controllingFactionRelation);
  }
  bool Read(RakNet::BitStream& bs, OverviewWorld& model) const override {
    auto addressSerializer = NetworkAddressSerializer::GetInstance();

    bs.ReadCompressed(model.id);
    addressSerializer.Read(bs, model.address);
    bs.ReadCompressed(model.playerCount);
    bs.ReadCompressed(model.controllingFaction);
    bs.ReadCompressed(model.controllingFactionRelation);
    return true;
  }
};


}  // namespace FOMNetwork
