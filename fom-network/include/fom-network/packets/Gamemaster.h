#pragma once

#include <fom-network/Interop.h>
#include <fom-network/types/Item.h>

namespace FOMNetwork {
namespace Packet {

// Every staff/GM command the client issues (kick, summon, spawn, …) arrives as
// this single packet; `command` is the discriminator (e.g. 0xd = spawn). Only
// the spawn command's tail (`item` + `quantity`) is modelled here, since that is
// the only command the server handles so far. Other commands still decode the
// common header and are ignored by the handler.
#pragma pack(push, 1)
struct Gamemaster {
  uint32_t playerId;
  uint16_t command;
  Type::Item item;
  uint32_t quantity;
};
#pragma pack(pop)

ASSERT_BLITTABLE(Gamemaster);

}  // namespace Packet
}  // namespace FOMNetwork
