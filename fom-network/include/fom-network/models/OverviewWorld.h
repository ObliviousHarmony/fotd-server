#pragma once

#include <fom-network/Common.h>
#include <fom-network/FOMNetworkEnums.h>
#include <fom-network/models/NetworkAddress.h>

namespace FOMNetwork {

#pragma pack(push, 1)
struct OverviewWorld {
  WorldID id;
  NetworkAddress address;
  uint16_t playerCount;
  Faction controllingFaction;
  FactionRelation controllingFactionRelation;
};
#pragma pack(pop)

ASSERT_BLITTABLE(OverviewWorld);

}  // namespace FOMNetwork
