#include <fom-network/packets/WorldObjectsPacket.h>

#include "../structs/world/WorldObjectInteropSerializer.h"
#include "../structs/world/WorldServiceControlInteropSerializer.h"
#include "../structs/world/WorldServiceInteropSerializer.h"
#include "PacketSerializers.h"

namespace FOMNetwork {

namespace {

void WriteObjects(RakNet::BitStream& bs,
                  const WorldObjectSerializer& serializer,
                  const WorldObjectInterop* objects, uint32_t count) {
  if (count > MAX_WORLD_OBJECTS) count = MAX_WORLD_OBJECTS;

  bs.WriteCompressed(count);
  for (uint32_t i = 0; i < count; ++i) serializer.Write(bs, objects[i]);
}

void WriteServices(RakNet::BitStream& bs,
                   const WorldServiceInteropSerializer& serializer,
                   const WorldServiceInterop* services, uint32_t count) {
  if (count > MAX_WORLD_SERVICES) count = MAX_WORLD_SERVICES;

  bs.WriteCompressed(count);
  for (uint32_t i = 0; i < count; ++i) serializer.Write(bs, services[i]);
}

void WriteControls(RakNet::BitStream& bs,
                   const WorldServiceControlSerializer& serializer,
                   const WorldServiceControlInterop* controls, uint32_t count) {
  if (count > MAX_WORLD_SERVICE_CONTROLS) count = MAX_WORLD_SERVICE_CONTROLS;

  bs.WriteCompressed(count);
  for (uint32_t i = 0; i < count; ++i) serializer.Write(bs, controls[i]);
}

}  // namespace

void WorldObjectsPacketSerializer::Write(RakNet::BitStream& bs,
                                         const WorldObjectsPacket* data) const {
  WorldObjectSerializer objectSerializer;
  WorldServiceInteropSerializer serviceSerializer;
  WorldServiceControlSerializer controlSerializer;

  bs.WriteCompressed(data->action);

  switch (data->action) {
    case WORLD_OBJECTS_ACTION_WORLD_SYNC: {
      const WorldObjectsPacket::WorldSync& sync = data->sync;

      WriteObjects(bs, objectSerializer, sync.backpacks, sync.numBackpacks);
      WriteObjects(bs, objectSerializer, sync.items, sync.numItems);
      WriteServices(bs, serviceSerializer, sync.services, sync.numServices);
      WriteObjects(bs, objectSerializer, sync.deployables, sync.numDeployables);
      WriteObjects(bs, objectSerializer, sync.influenceGenerators,
                   sync.numInfluenceGenerators);
      WriteObjects(bs, objectSerializer, sync.medicalUnits,
                   sync.numMedicalUnits);
      WriteObjects(bs, objectSerializer, sync.miningRigs, sync.numMiningRigs);
      WriteControls(bs, controlSerializer, sync.serviceControls,
                    sync.numServiceControls);

      // NPC Count
      bs.WriteCompressed((uint16_t)0);

      WriteObjects(bs, objectSerializer, sync.territoryObjects,
                   sync.numTerritoryObjects);
      WriteObjects(bs, objectSerializer, sync.explosives, sync.numExplosives);
      break;
    }

    case WORLD_OBJECTS_ACTION_OBJECT_SYNC:
      bs.WriteCompressed(data->type);

      // The wire format supports a counted batch of same-typed objects, but
      // the server sends one object per packet.
      switch (data->type) {
        case Enum::WORLD_OBJECT_BACKPACK:
        case Enum::WORLD_OBJECT_ITEM:
        case Enum::WORLD_OBJECT_DEPLOYABLE:
        case Enum::WORLD_OBJECT_INFLUENCE_GENERATOR:
        case Enum::WORLD_OBJECT_MEDICAL_UNIT:
        case Enum::WORLD_OBJECT_MINING_RIG:
        case Enum::WORLD_OBJECT_TERRITORY_OBJECT:
        case Enum::WORLD_OBJECT_EXPLOSIVE_CHARGE:
          bs.WriteCompressed((uint32_t)1);
          objectSerializer.Write(bs, data->object);
          break;
        case Enum::WORLD_OBJECT_SERVICE:
          bs.WriteCompressed((uint32_t)1);
          serviceSerializer.Write(bs, data->service);
          break;
        case Enum::WORLD_OBJECT_SERVICE_CONTROL:
          bs.WriteCompressed((uint32_t)1);
          controlSerializer.Write(bs, data->control);
          break;
        case Enum::WORLD_OBJECT_NPC:
          bs.WriteCompressed((uint16_t)0);
          break;
      }
      break;

    case WORLD_OBJECTS_ACTION_SET_ACTIVE:
      bs.WriteCompressed(data->type);
      bs.WriteCompressed(data->objectId);
      bs.Write(data->isActive == 1);
      break;

    case WORLD_OBJECTS_ACTION_CHANGE_STATE:
      bs.WriteCompressed(data->type);
      bs.WriteCompressed(data->objectId);
      bs.WriteCompressed(data->state);
      break;
  }
}

}  // namespace FOMNetwork
