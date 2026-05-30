#pragma once

#include <fom-network/Interop.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct Login {
  uint8_t username[BufferSizes::USERNAME];
  uint8_t passwordHash[64];
  uint32_t clientCrc;
  uint32_t cshellCrc;
  uint32_t objectCrc;
  uint8_t macAddress[32];
  uint8_t driveModels[4][64];
  uint8_t driveSerialNumbers[4][32];
  uint8_t loginToken[64];
  uint8_t computerName[32];

  uint8_t hasSteamTicket;
  int32_t steamTicketLength;  // hasSteamTicket == 1
  uint8_t steamTicket[1024];  // hasSteamTicket == 1
};
#pragma pack(pop)

ASSERT_BLITTABLE(Login);

}  // namespace Packet
}  // namespace FOMNetwork
