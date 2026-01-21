#pragma once

#include <fom-network/types/NetworkAddress.h>

#include "TypeSerializer.h"

namespace FOMNetwork {

class NetworkAddressSerializer
    : protected TypeSerializer<Type::NetworkAddress> {
 public:
  void Write(RakNet::BitStream& bs, const Type::NetworkAddress& data) const {
    // The client expects the binary address to be bitwise NOTed and
    // endian-swapped.
    uint32_t bitStreamAddress = data.binaryAddress;
    bitStreamAddress = ~bitStreamAddress;
    bs.ReverseBytesInPlace((uint8_t*)&bitStreamAddress, 4);

    bs.Write(bitStreamAddress);
    bs.Write(data.port);
  }

  bool Read(RakNet::BitStream& bs, Type::NetworkAddress& data) const {
    // Reverse the bitwise NOT and endian-swap.
    uint32_t bitStreamAddress;
    if (!bs.Read(bitStreamAddress)) return false;
    bs.ReverseBytesInPlace((uint8_t*)&bitStreamAddress, 4);
    bitStreamAddress = ~bitStreamAddress;

    data.binaryAddress = bitStreamAddress;
    if (!bs.Read(data.port)) return false;
    return true;
  }
};

}  // namespace FOMNetwork
