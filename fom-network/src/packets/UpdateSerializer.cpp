#include <fom-network/packets/Update.h>

#include "../types/WorldUpdateSerializer.h"
#include "PacketSerializers.h"

namespace FOMNetwork {
namespace Packet {

bool UpdateSerializer::Read(RakNet::BitStream& bs, Packet::Update* data) const {
  Type::WorldUpdateSerializer worldUpdateSerializer;
  return worldUpdateSerializer.Read(bs, data->update);
}

}  // namespace Packet
}  // namespace FOMNetwork
