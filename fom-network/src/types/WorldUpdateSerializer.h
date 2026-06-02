#pragma once

#include <fom-network/types/WorldUpdate.h>

#include "AvatarSerializer.h"
#include "PositionRotationSerializer.h"
#include "PositionSerializer.h"
#include "TypeSerializer.h"

namespace FOMNetwork {
namespace Type {

class WorldUpdateSerializer : protected TypeSerializer<Type::WorldUpdate> {
 public:
  void Write(RakNet::BitStream& bs, const Type::WorldUpdate& data) const;
  bool Read(RakNet::BitStream& bs, Type::WorldUpdate& data) const;

 private:
  void WritePlayer(RakNet::BitStream& bs, const Type::WorldUpdate& data) const;
  bool ReadPlayer(RakNet::BitStream& bs, Type::WorldUpdate& data) const;

  void WriteCharacter(RakNet::BitStream& bs,
                      const Type::WorldUpdate& data) const;
  bool ReadCharacter(RakNet::BitStream& bs, Type::WorldUpdate& data) const;
};

}  // namespace Type
}  // namespace FOMNetwork
