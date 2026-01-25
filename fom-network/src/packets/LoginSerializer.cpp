#include <fom-network/packets/Login.h>

#include "PacketSerializers.h"

namespace FOMNetwork {

bool LoginSerializer::Read(RakNet::BitStream& bs, Packet::Login* data) const {
  if (!DecodeString(bs, data->username)) return false;
  if (!ReadString(bs, data->passwordHash)) return false;
  if (!bs.Read(data->clientCRC)) return false;
  if (!bs.Read(data->cshellCRC)) return false;
  if (!bs.Read(data->objectCRC)) return false;
  if (!DecodeString(bs, data->macAddress)) return false;

  for (int i = 0; i < 4; i++) {
    if (!ReadString(bs, data->driveModels[i])) return false;
    if (!ReadString(bs, data->driveSerialNumbers[i])) return false;
  }

  if (!ReadString(bs, data->loginToken)) return false;
  if (!DecodeString(bs, data->computerName)) return false;

  bool hasSteamTicket;
  if (!bs.Read(hasSteamTicket)) return false;
  data->hasSteamTicket = hasSteamTicket ? 1 : 0;
  if (data->hasSteamTicket == 1) {
    for (int i = 0; i < 1024; i++) {
      if (!bs.ReadCompressed(data->steamTicket[i])) return false;
    }
    if (!bs.ReadCompressed(data->steamTicketLength)) return false;
  }

  return true;
}

}  // namespace FOMNetwork
