#pragma once

#include <fom-network/types/NetworkAddress.h>

#include "TypeSerializer.h"

namespace FOMNetwork {

class NetworkAddressSerializer
    : protected TypeSerializer<Type::NetworkAddress> {
 public:
  void Write(RakNet::BitStream& bs, const Type::NetworkAddress& data) const {
    bs.WriteCompressed(data.binaryAddress);
    bs.WriteCompressed(data.port);
  }

  bool Read(RakNet::BitStream& bs, Type::NetworkAddress& data) const {
    if (!bs.ReadCompressed(data.binaryAddress)) return false;
    if (!bs.ReadCompressed(data.port)) return false;
    return true;
  }
};

}  // namespace FOMNetwork
