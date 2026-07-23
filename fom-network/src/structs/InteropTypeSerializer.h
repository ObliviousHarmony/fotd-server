#pragma once

#include "../BaseSerializer.h"

namespace FOMNetwork {

template <typename Type>
class InteropTypeSerializer : protected BaseSerializer {
 public:
  virtual void Write(RakNet::BitStream& bs, const Type& data) const = 0;
  virtual bool Read(RakNet::BitStream& bs, Type& data) const = 0;
};

}  // namespace FOMNetwork
