#include <fom-network/FOMDataSerializer.h>
#include <fom-network/NetworkAPI.h>
#include <fom-network/packets/PacketTypes.h>

#include <unordered_map>
#include <unordered_set>

using namespace FOMNetwork;
using namespace FOMNetwork::Packet;

int32_t FOMNetwork_ValidatePacketStructs(const PacketStructure* structures,
                                         int32_t count) {
  // Both should have the same number of packets defined.
  if (FOMDataSerializer::PacketSizes.size() != count) {
    return -1;
  }

  // Make sure that the size of each packet ID matches.
  // This is not a comprehensive check but the
  // consumer can perform any additional
  // verification that may be needed.
  for (int32_t i = 0; i < count; i++) {
    auto it = FOMDataSerializer::PacketSizes.find(structures[i].id);
    if (it == FOMDataSerializer::PacketSizes.end()) {
      return -2;
    }

    if (it->second != structures[i].size) {
      return -3;
    }
  }

  return 0;
}
