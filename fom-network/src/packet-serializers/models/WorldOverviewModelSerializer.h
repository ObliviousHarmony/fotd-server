#pragma once

#include <fom-network/packets/models/WorldOverviewModel.h>

#include "../NetworkAddressSerializer.h"
#include "ApartmentModelSerializer.h"
#include "ModelSerializer.h"

namespace FOMNetwork {

class WorldOverviewModelSerializer
    : public ModelSerializer<Packet::WorldOverviewModel> {
 public:
  void Write(RakNet::BitStream& bs,
             const Packet::WorldOverviewModel& model) const override {
    NetworkAddressSerializer addressSerializer;
    ApartmentModelSerializer apartmentSerializer;

    bs.WriteCompressed(model.numWorlds);
    for (size_t i = 0; i < model.numWorlds && i < NUM_WORLDS; i++) {
      auto& world = model.worldBuffer[i];

      bs.WriteCompressed(world.id);
      addressSerializer.Write(bs, world.address);
      bs.WriteCompressed(world.playerCount);
      bs.WriteCompressed(world.controllingFaction);
      bs.WriteCompressed(world.controllingFactionRelation);
    }

    bs.WriteCompressed(model.onlinePlayers);
    bs.Write0();                     // Show Training Grounds
    bs.WriteCompressed((uint8_t)0);  // Unknown Byte
    bs.Write(model.isPrisoner == 1);
    bs.Write0();  // Unknown Flag
    bs.WriteCompressed(model.onlineNewPlayers);
    apartmentSerializer.Write(bs, model.defaultApartment);
  }
  bool Read(RakNet::BitStream& bs,
            Packet::WorldOverviewModel& model) const override {
    NetworkAddressSerializer addressSerializer;
    ApartmentModelSerializer apartmentSerializer;

    bs.ReadCompressed(model.numWorlds);
    for (size_t i = 0; i < model.numWorlds && i < NUM_WORLDS; i++) {
      auto& world = model.worldBuffer[i];

      bs.ReadCompressed(world.id);
      addressSerializer.Write(bs, world.address);
      bs.ReadCompressed(world.playerCount);
      bs.ReadCompressed(world.controllingFaction);
      bs.ReadCompressed(world.controllingFactionRelation);
    }

    bs.ReadCompressed(model.onlinePlayers);
    bs.IgnoreBits(1);  // Show Training Grounds
    bs.IgnoreBytes(1);
    model.isPrisoner = bs.ReadBit() ? 1 : 0;
    bs.IgnoreBits(1);  // Unknown Flag
    bs.ReadCompressed(model.onlineNewPlayers);
    apartmentSerializer.Read(bs, model.defaultApartment);
    return true;
  }
};

}  // namespace FOMNetwork
