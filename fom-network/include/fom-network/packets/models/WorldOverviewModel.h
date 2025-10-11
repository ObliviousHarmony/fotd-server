#pragma once
#pragma once

#include <fom-network/Common.h>
#include <fom-network/FOMNetworkTypes.h>
#include <fom-network/packets/NetworkAddress.h>
#include <fom-network/enums/WorldID.h>
#include <fom-network/enums/Faction.h>
#include <fom-network/enums/FactionRelation.h>
#include <fom-network/packets/models/ApartmentModel.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct WorldOverviewModelEntry {
  Enums::WorldID id;
  NetworkAddress address;
  uint16_t playerCount;
  Enums::Faction controllingFaction;
  Enums::FactionRelation controllingFactionRelation;
};
#pragma pack(pop)

ASSERT_BLITTABLE(WorldOverviewModelEntry);

#pragma pack(push, 1)
struct WorldOverviewModel {
  uint8_t numWorlds;
  WorldOverviewModelEntry worldBuffer[Enums::NUM_WORLDS];
  uint32_t onlinePlayers;
  uint32_t onlineNewPlayers;
  uint8_t isPrisoner;
  ApartmentModel defaultApartment;
};
#pragma pack(pop)

ASSERT_BLITTABLE(WorldOverviewModel);

}  // namespace Packet
}  // namespace FOMNetwork
