#pragma once

#include "ModelSerializer.h"

namespace FOMNetwork {

class NetworkAddressSerializer
    : public ModelSerializer<NetworkAddressSerializer, NetworkAddress> {
 public:
  void Write(RakNet::BitStream& bs,
             const NetworkAddress& model) const override {
    bs.Write(model.binaryAddress);
    bs.Write(model.port);
  }
  void Read(RakNet::BitStream& bs, NetworkAddress& model) const override {
    bs.Read(model.binaryAddress);
    bs.Read(model.port);
  }
};

}  // namespace FOMNetwork
