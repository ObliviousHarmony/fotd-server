#pragma once
#pragma once

#include <fom-network/Common.h>
#include <fom-network/FOMNetworkEnums.h>
#include <fom-network/packets/NetworkAddress.h>
#include <fom-network/packets/models/ApartmentModel.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct WorldOverviewModelEntry {
  WorldID id;
  NetworkAddress address;
  uint16_t playerCount;
  Faction controllingFaction;
  FactionRelation controllingFactionRelation;
};
#pragma pack(pop)

ASSERT_BLITTABLE(WorldOverviewModelEntry);

#pragma pack(push, 1)
struct WorldOverviewModel {
  uint8_t numWorlds;
  WorldOverviewModelEntry worldBuffer[NUM_WORLDS];
  uint32_t onlinePlayers;
  uint32_t onlineNewPlayers;
  uint8_t isPrisoner;
  ApartmentModel defaultApartment;
};
#pragma pack(pop)

ASSERT_BLITTABLE(WorldOverviewModel);

}  // namespace Packet
}  // namespace FOMNetwork
