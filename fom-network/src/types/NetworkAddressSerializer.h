#pragma once

#include <fom-network/types/NetworkAddress.h>

#include "TypeSerializer.h"

namespace FOMNetwork {

class NetworkAddressSerializer
    : protected TypeSerializer<Type::NetworkAddress> {
 public:
  void Write(RakNet::BitStream& bs, const Type::NetworkAddress& model) const {
    // The client expects the binary address to be bitwise NOTed and
    // endian-swapped.
    uint32_t bitStreamAddress = model.binaryAddress;
    bitStreamAddress = ~bitStreamAddress;
    bs.ReverseBytesInPlace((uint8_t*)&bitStreamAddress, 4);

    bs.Write(bitStreamAddress);
    bs.Write(model.port);
  }

  bool Read(RakNet::BitStream& bs, Type::NetworkAddress& model) const {
    // Reverse the bitwise NOT and endian-swap.
    uint32_t bitStreamAddress;
    if (!bs.Read(bitStreamAddress)) return false;
    bs.ReverseBytesInPlace((uint8_t*)&bitStreamAddress, 4);
    bitStreamAddress = ~bitStreamAddress;

    model.binaryAddress = bitStreamAddress;
    if (!bs.Read(model.port)) return false;
    return true;
  }
};

}  // namespace FOMNetwork
