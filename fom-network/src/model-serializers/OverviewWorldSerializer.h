#pragma once

#include <fom-network/models/OverviewWorld.h>

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
    addressSerializer.Read(bs, model.address)
        bs.ReadCompressed(model.playerCount);
    bs.ReadCompressed(model.controllingFaction);
    bs.ReadCompressed(model.controllingFactionRelation);
    return true;
  }
};

bs.WriteByteCompressed(_servers[index].Item1);  // world ID
bs.WriteAddress(new IPEndPoint(IPAddress.Parse(_servers[index].Item2),
                               _servers[index].Item3));
bs.WriteUShortCompressed(5, Endian.Big);  // # of players on world
// faction in control of world, 0 = nothing, 1 = LED/FDC, 2 = LED/FDC, 3 = GoM,
// 4 = BoS, 5 = MotB, 6 = CMG, 7 = EC, 8 = Vortex Inc.
bs.WriteByteCompressed(info[server.Item1].Item2);
bs.WriteByteCompressed(
    info[server.Item1]
        .Item3);  // faction relationship status, 0 = nothing, 1 = peaceful?
                  // (green hands), 2 = neutral? (blue hands), 3 = nothing?, 4 =
                  // warning? (yellow exclamation point), 5 = hostile? (red
                  // exclamation point)

}  // namespace FOMNetwork
