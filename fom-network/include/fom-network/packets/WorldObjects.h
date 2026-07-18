#pragma once

#include <fom-network/Interop.h>
#include <fom-network/enums/WorldObjectType.h>
#include <fom-network/types/world/WorldObject.h>
#include <fom-network/types/world/WorldService.h>
#include <fom-network/types/world/WorldServiceControl.h>

namespace FOMNetwork {
namespace Packet {

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
struct WorldObjects {
  // Collections are in wire order. NPCs (WORLD_OBJECT_NPC) are not modeled
  // yet; the serializer writes an empty collection in their place.
  struct WorldSync {
    uint32_t numBackpacks;
    Type::WorldObject backpacks[MAX_WORLD_OBJECTS];
    uint32_t numItems;
    Type::WorldObject items[MAX_WORLD_OBJECTS];
    uint32_t numServices;
    Type::WorldService services[MAX_WORLD_SERVICES];
    uint32_t numDeployables;
    Type::WorldObject deployables[MAX_WORLD_OBJECTS];
    uint32_t numInfluenceGenerators;
    Type::WorldObject influenceGenerators[MAX_WORLD_OBJECTS];
    uint32_t numMedicalUnits;
    Type::WorldObject medicalUnits[MAX_WORLD_OBJECTS];
    uint32_t numMiningRigs;
    Type::WorldObject miningRigs[MAX_WORLD_OBJECTS];
    uint32_t numServiceControls;
    Type::WorldServiceControl serviceControls[MAX_WORLD_SERVICE_CONTROLS];
    uint32_t numTerritoryObjects;
    Type::WorldObject territoryObjects[MAX_WORLD_OBJECTS];
    uint32_t numExplosives;
    Type::WorldObject explosives[MAX_WORLD_OBJECTS];
  };

  WorldObjectsAction action;
  Enum::WorldObjectType type;  // OBJECT_SYNC / SET_ACTIVE / CHANGE_STATE
  uint32_t objectId;           // SET_ACTIVE / CHANGE_STATE
  uint8_t isActive;            // SET_ACTIVE
  uint8_t state;               // CHANGE_STATE
  union {
    WorldSync sync;                     // WORLD_SYNC
    Type::WorldObject object;           // OBJECT_SYNC (generic types)
    Type::WorldService service;         // OBJECT_SYNC (SERVICE)
    Type::WorldServiceControl control;  // OBJECT_SYNC (SERVICE_CONTROL)
  };
};
#pragma pack(pop)

ASSERT_BLITTABLE(WorldObjects);

}  // namespace Packet
}  // namespace FOMNetwork
