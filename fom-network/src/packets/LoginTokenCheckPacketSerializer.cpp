#include <fom-network/packets/LoginTokenCheckPacket.h>

#include "PacketSerializers.h"

namespace FOMNetwork {

bool LoginTokenCheckPacketSerializer::Read(RakNet::BitStream& bs,
                                           LoginTokenCheckPacket* data) const {
  bool fromServer;
  if (!bs.Read(fromServer)) return false;
  data->fromServer = fromServer ? 1 : 0;

  if (data->fromServer == 1) {
    bool success;
    if (!bs.Read(success)) return false;
    data->success = success ? 1 : 0;
    if (!ReadString(bs, data->username)) return false;
  } else {
    if (!ReadString(bs, data->requestToken)) return false;
  }

  return true;
}

void LoginTokenCheckPacketSerializer::Write(
    RakNet::BitStream& bs, const LoginTokenCheckPacket* data) const {
  bs.Write(data->fromServer == 1);
  if (data->fromServer == 1) {
    bs.Write(data->success == 1);
    WriteString(bs, data->username);
  } else {
    WriteString(bs, data->requestToken);
  }
}

}  // namespace FOMNetwork
