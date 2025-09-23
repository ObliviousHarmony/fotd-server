#include <fom-network/FOMPacket.h>
#include <fom-network/NetworkAPI.h>

#include <unordered_map>
#include <unordered_set>

int32_t FOMNetwork_ValidatePacketStructs(const PacketStructure* structures,
                                         int32_t count) {
#pragma warning(push)
#pragma warning(disable : 4267)
  // List all of the structs that we have defined in the library
  // so that they can be compared to the consumer's structs.
  std::unordered_map<uint8_t, uint32_t> libraryMap = {
      {ID_FOM_PACKET_READ_ERROR, sizeof(FOMPacket::ReadPacketError)},
      {ID_LOGIN_REQUEST, sizeof(FOMPacket::LoginRequest)},
      {ID_LOGIN_REQUEST_RETURN, sizeof(FOMPacket::LoginRequestReturn)},
      {ID_LOGIN, sizeof(FOMPacket::Login)}};
#pragma warning(pop)

  // Both should have the same number of packets defined.
  if (libraryMap.size() != count) {
    return -1;
  }

  // Make sure that the size of each packet ID matches.
  // This is not a comprehensive check but the
  // consumer can perform any additional
  // verification that may be needed.
  for (int32_t i = 0; i < count; i++) {
    auto it = libraryMap.find(structures[i].id);
    if (it == libraryMap.end()) {
      return -2;
    }

    if (it->second != structures[i].size) {
      return -3;
    }
  }

  return 0;
}
