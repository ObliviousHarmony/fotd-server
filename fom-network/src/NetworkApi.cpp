#include <fom-network/NetworkApi.h>

#include "FOMDataSerializer.h"

using namespace FOMNetwork;

int32_t FOMNetwork_ValidatePacketStructs(const PacketStructure* structures,
                                         int32_t count) {
  // Both should have the same number of packets defined.
  if (FOMDataSerializer::GetPacketCount() != static_cast<size_t>(count)) {
    return -1;
  }

  // Make sure that the size of each packet Id matches.
  // This is not a comprehensive check but the
  // consumer can perform any additional
  // verification that may be needed.
  for (int32_t i = 0; i < count; i++) {
    int packetSize = FOMDataSerializer::GetPacketSize(structures[i].id);
    if (packetSize < 0) {
      return -2;
    }

    if (static_cast<size_t>(packetSize) != structures[i].size) {
      return -3;
    }
  }

  return 0;
}
