#pragma once

#include <fom-network/packets/PacketSerializers.h>

namespace FOMNetwork {

template <typename Model>
class ModelSerializer : protected BaseSerializer {
 public:
  virtual void Write(RakNet::BitStream& bs, const Model& model) const = 0;
  virtual bool Read(RakNet::BitStream& bs, Model& model) const = 0;
};

}  // namespace FOMNetwork
