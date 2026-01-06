#include <fom-network/packets/LoginTokenCheck.h>

#include "PacketSerializers.h"

namespace FOMNetwork {

bool LoginTokenCheckSerializer::Read(RakNet::BitStream& bs,
                                     Packet::LoginTokenCheck* data) const {
  data->fromServer = bs.ReadBit() ? 1 : 0;
  if (data->fromServer == 1) {
    data->success = bs.ReadBit() ? 1 : 0;
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

}  // namespace FOMNetwork
