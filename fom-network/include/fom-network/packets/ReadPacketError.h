#pragma once

#include <fom-network/Common.h>

namespace FOMNetwork {
namespace Packet {

enum ReadPacketErrorCode : uint8_t {
  ERROR_MISSING_PACKET_ID,
  ERROR_UNHANDLED_PACKET_ID,
  ERROR_READ
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
