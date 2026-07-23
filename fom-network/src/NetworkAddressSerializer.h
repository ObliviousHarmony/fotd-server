#pragma once

#include <fom-network/NetworkAddress.h>

#include "structs/InteropTypeSerializer.h"

namespace FOMNetwork {

class NetworkAddressSerializer
    : protected InteropTypeSerializer<NetworkAddress> {
 public:
  void Write(RakNet::BitStream& bs, const NetworkAddress& data) const {
    bs.WriteCompressed(data.binaryAddress);
    bs.WriteCompressed(data.port);
  }

  bool Read(RakNet::BitStream& bs, NetworkAddress& data) const {
    if (!bs.ReadCompressed(data.binaryAddress)) return false;
    if (!bs.ReadCompressed(data.port)) return false;
    return true;
  }
};

}  // namespace FOMNetwork
