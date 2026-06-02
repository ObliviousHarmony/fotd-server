#include <fom-network/packets/LoginTokenCheck.h>

#include "PacketSerializers.h"

namespace FOMNetwork {
namespace Packet {

bool LoginTokenCheckSerializer::Read(RakNet::BitStream& bs,
                                     Packet::LoginTokenCheck* data) const {
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

void LoginTokenCheckSerializer::Write(
    RakNet::BitStream& bs, const Packet::LoginTokenCheck* data) const {
  bs.Write(data->fromServer == 1);
  if (data->fromServer == 1) {
    bs.Write(data->success == 1);
    WriteString(bs, data->username);
  } else {
    WriteString(bs, data->requestToken);
  }
}

}  // namespace Packet
}  // namespace FOMNetwork
