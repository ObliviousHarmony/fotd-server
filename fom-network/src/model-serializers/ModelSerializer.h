#pragma once

#include <fom-network/PacketSerializers.h>

namespace FOMNetwork {

template <typename Derived, typename Model>
class ModelSerializer : protected BaseSerializer {
 public:
  static Derived& GetInstance() {
    static Derived s;
    return s;
  }

  virtual void Write(RakNet::BitStream& bs, const Model& model) const = 0;
  virtual bool Read(RakNet::BitStream& bs, Model& model) const = 0;
};

}  // namespace FOMNetwork
