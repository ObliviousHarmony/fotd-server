#include <fom-network/packets/Gamemaster.h>

#include "../types/ItemSerializer.h"
#include "PacketSerializers.h"

namespace FOMNetwork {
namespace Packet {

namespace {
// Command discriminator for /spawn (see the client's FUN_100fe3a0 switch).
constexpr uint16_t kCommandSpawn = 0xd;
}  // namespace

bool GamemasterSerializer::Read(RakNet::BitStream& bs,
                                Packet::Gamemaster* data) const {
  if (!bs.ReadCompressed(data->playerId)) return false;
  if (!bs.ReadCompressed(data->command)) return false;

  // Only the spawn command's tail is understood. Other commands share this
  // packet id but carry a different (unmodelled) tail, so leave `item`/`quantity`
  // zeroed and let the managed handler ignore them by `command`.
  if (data->command == kCommandSpawn) {
    Type::ItemSerializer itemSerializer;
    if (!itemSerializer.Read(bs, data->item)) return false;
    if (!bs.ReadCompressed(data->quantity)) return false;
  }

  return true;
}

}  // namespace Packet
}  // namespace FOMNetwork
