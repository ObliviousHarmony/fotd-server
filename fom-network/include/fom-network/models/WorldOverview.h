#pragma once
#pragma once

#include <fom-network/Common.h>
#include <fom-network/FOMNetworkEnums.h>
#include <fom-network/models/Apartment.h>
#include <fom-network/models/NetworkAddress.h>

namespace FOMNetwork {

#pragma pack(push, 1)
struct WorldOverviewEntry {
  WorldID id;
  NetworkAddress address;
  uint16_t playerCount;
  Faction controllingFaction;
  FactionRelation controllingFactionRelation;
};
#pragma pack(pop)

ASSERT_BLITTABLE(WorldOverviewEntry);

#pragma pack(push, 1)
struct WorldOverview {
  uint8_t numWorlds;
  WorldOverviewEntry worldBuffer[NUM_WORLDS];
  uint32_t onlinePlayers;
  uint32_t onlineNewPlayers;
  uint8_t isPrisoner;
  Apartment defaultApartment;
};
#pragma pack(pop)

ASSERT_BLITTABLE(WorldOverview);

}  // namespace FOMNetwork
