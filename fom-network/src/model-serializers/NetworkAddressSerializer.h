#pragma once

#include <fom-network/models/NetworkAddress.h>

#include "ModelSerializer.h"

namespace FOMNetwork {

class NetworkAddressSerializer
    : public ModelSerializer<NetworkAddress> {
 public:
  void Write(RakNet::BitStream& bs,
             const NetworkAddress& model) const override {
    bs.Write(model.binaryAddress);
    bs.Write(model.port);
  }
  bool Read(RakNet::BitStream& bs, NetworkAddress& model) const override {
    bs.Read(model.binaryAddress);
    bs.Read(model.port);
    return true;
  }
};

}  // namespace FOMNetwork
