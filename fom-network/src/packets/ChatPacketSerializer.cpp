#include <fom-network/packets/ChatPacket.h>

#include "PacketSerializers.h"

namespace FOMNetwork {

bool ChatPacketSerializer::Read(RakNet::BitStream& bs, ChatPacket* data) const {
  if (!bs.ReadCompressed(data->channel)) return false;
  if (!bs.ReadCompressed(data->senderId)) return false;

  switch (data->channel) {
    case Enum::CHAT_PRIVATE:
    case Enum::CHAT_TRADE:
    case Enum::CHAT_GM:
      if (!bs.ReadCompressed(data->targetId)) return false;
      break;
  }

  if (data->channel != Enum::CHAT_SYSTEM) {
    if (bs.ReadBit()) {
      if (!bs.ReadCompressed(data->chatStyle)) return false;
    } else
      data->chatStyle = 0;

    if (!DecodeString(bs, data->senderName)) return false;
  }

  if (!DecodeString(bs, data->message)) return false;

  return true;
}

void ChatPacketSerializer::Write(RakNet::BitStream& bs,
                                 const ChatPacket* data) const {
  bs.WriteCompressed(data->channel);
  bs.WriteCompressed(data->senderId);

  switch (data->channel) {
    case Enum::CHAT_PRIVATE:
    case Enum::CHAT_TRADE:
    case Enum::CHAT_GM:
      bs.WriteCompressed(data->targetId);
      break;
  }

  if (data->channel != Enum::CHAT_SYSTEM) {
    if (data->chatStyle) {
      bs.Write1();
      bs.WriteCompressed(data->chatStyle);
    } else
      bs.Write0();

    EncodeString(bs, data->senderName);
  }

  EncodeString(bs, data->message);
}

}  // namespace FOMNetwork
