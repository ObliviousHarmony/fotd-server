#include <fom-network/packets/UpdatePacket.h>

#include "../structs/WorldUpdateInteropSerializer.h"
#include "PacketSerializers.h"

namespace FOMNetwork {

bool UpdatePacketSerializer::Read(RakNet::BitStream& bs,
                                  UpdatePacket* data) const {
  WorldUpdateInteropSerializer worldUpdatePacketSerializer;
  return worldUpdatePacketSerializer.Read(bs, data->update);
}

}  // namespace FOMNetwork
