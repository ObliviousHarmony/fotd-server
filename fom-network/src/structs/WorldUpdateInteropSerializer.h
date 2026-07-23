#pragma once

#include <fom-network/structs/WorldUpdateInterop.h>

#include "AvatarInteropSerializer.h"
#include "InteropTypeSerializer.h"
#include "PositionInteropSerializer.h"
#include "PositionRotationInteropSerializer.h"

namespace FOMNetwork {

class WorldUpdateInteropSerializer
    : protected InteropTypeSerializer<WorldUpdateInterop> {
 public:
  void Write(RakNet::BitStream& bs, const WorldUpdateInterop& data) const;
  bool Read(RakNet::BitStream& bs, WorldUpdateInterop& data) const;

 private:
  void WritePlayer(RakNet::BitStream& bs,
                   const WorldUpdateInterop::PlayerUpdate& data) const;
  bool ReadPlayer(RakNet::BitStream& bs,
                  WorldUpdateInterop::PlayerUpdate& data) const;

  void WriteCharacter(RakNet::BitStream& bs,
                      const WorldUpdateInterop::CharacterUpdate& data) const;
  bool ReadCharacter(RakNet::BitStream& bs,
                     WorldUpdateInterop::CharacterUpdate& data) const;
};

}  // namespace FOMNetwork
