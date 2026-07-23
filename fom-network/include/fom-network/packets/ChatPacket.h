#pragma once

#include <fom-network/Interop.h>
#include <fom-network/enums/ChatChannel.h>

namespace FOMNetwork {

#pragma pack(push, 1)
struct ChatPacket {
  Enum::ChatChannel channel;
  uint32_t senderId;
  uint32_t targetId;  // channel == CHAT_PRIVATE || CHAT_TRADE || CHAT_GM
  uint8_t chatStyle;  // channel != CHAT_SYSTEM
  uint8_t senderName[BufferSizes::PLAYER_NAME];
  uint8_t message[400];
};
#pragma pack(pop)

ASSERT_BLITTABLE(ChatPacket);

}  // namespace FOMNetwork
