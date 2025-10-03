#pragma once

#include <fom-network/Common.h>

namespace FOMNetwork {
namespace Packet {

enum ReadPacketErrorCode : uint8_t {
  ERROR_MISSING_PACKET_ID = 0,
  ERROR_UNHANDLED_PACKET_ID = 1,
  ERROR_READ = 2
};

#pragma pack(push, 1)
struct ReadPacketError {
  PacketIdentifier offendingID;
  ReadPacketErrorCode errorCode;
};
#pragma pack(pop)

ASSERT_BLITTABLE(ReadPacketError);

}  // namespace Packet
}  // namespace FOMNetwork
