#pragma once

#include <fom-network/Interop.h>
#include <fom-network/enums/WorldObjectType.h>
#include <fom-network/structs/world/WorldObjectInterop.h>
#include <fom-network/structs/world/WorldServiceControlInterop.h>
#include <fom-network/structs/world/WorldServiceInterop.h>

namespace FOMNetwork {

enum WorldObjectsAction : uint8_t {
  WORLD_OBJECTS_ACTION_INVALID = 0,
  WORLD_OBJECTS_ACTION_WORLD_SYNC = 1,
  WORLD_OBJECTS_ACTION_OBJECT_SYNC = 2,
  WORLD_OBJECTS_ACTION_SET_ACTIVE = 3,
  WORLD_OBJECTS_ACTION_CHANGE_STATE = 4,
};

constexpr int32_t MAX_WORLD_OBJECTS = 512;
constexpr int32_t MAX_WORLD_SERVICES = 32;
constexpr int32_t MAX_WORLD_SERVICE_CONTROLS = 32;

#pragma pack(push, 1)
struct WorldObjectsPacket {
  // Collections are in wire order. NPCs (WORLD_OBJECT_NPC) are not modeled
  // yet; the serializer writes an empty collection in their place.
  struct WorldSync {
    uint32_t numBackpacks;
    WorldObjectInterop backpacks[MAX_WORLD_OBJECTS];
    uint32_t numItems;
    WorldObjectInterop items[MAX_WORLD_OBJECTS];
    uint32_t numServices;
    WorldServiceInterop services[MAX_WORLD_SERVICES];
    uint32_t numDeployables;
    WorldObjectInterop deployables[MAX_WORLD_OBJECTS];
    uint32_t numInfluenceGenerators;
    WorldObjectInterop influenceGenerators[MAX_WORLD_OBJECTS];
    uint32_t numMedicalUnits;
    WorldObjectInterop medicalUnits[MAX_WORLD_OBJECTS];
    uint32_t numMiningRigs;
    WorldObjectInterop miningRigs[MAX_WORLD_OBJECTS];
    uint32_t numServiceControls;
    WorldServiceControlInterop serviceControls[MAX_WORLD_SERVICE_CONTROLS];
    uint32_t numTerritoryObjects;
    WorldObjectInterop territoryObjects[MAX_WORLD_OBJECTS];
    uint32_t numExplosives;
    WorldObjectInterop explosives[MAX_WORLD_OBJECTS];
  };

  WorldObjectsAction action;
  Enum::WorldObjectType type;  // OBJECT_SYNC / SET_ACTIVE / CHANGE_STATE
  uint32_t objectId;           // SET_ACTIVE / CHANGE_STATE
  uint8_t isActive;            // SET_ACTIVE
  uint8_t state;               // CHANGE_STATE
  union {
    WorldSync sync;                      // WORLD_SYNC
    WorldObjectInterop object;           // OBJECT_SYNC (generic types)
    WorldServiceInterop service;         // OBJECT_SYNC (SERVICE)
    WorldServiceControlInterop control;  // OBJECT_SYNC (SERVICE_CONTROL)
  };
};
#pragma pack(pop)

ASSERT_BLITTABLE(WorldObjectsPacket);

}  // namespace FOMNetwork
