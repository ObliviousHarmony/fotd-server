#include <fom-network/packets/PacketSerializers.h>

#include "../NetworkAddressSerializer.h"

namespace FOMNetwork {

bool RegisterWorldSerializer::ReadData(RakNet::BitStream& bs,
                                       Packet::RegisterWorld& data) const {
  NetworkAddressSerializer addressSerializer;

  bs.ReadCompressed(data.worldID);
  addressSerializer.Read(bs, data.clientAddress);

  return true;
}

void RegisterWorldSerializer::WriteData(
    RakNet::BitStream& bs, const Packet::RegisterWorld& data) const {
  NetworkAddressSerializer addressSerializer;

  bs.WriteCompressed(data.worldID);
  addressSerializer.Write(bs, data.clientAddress);
}

}  // namespace FOMNetwork
