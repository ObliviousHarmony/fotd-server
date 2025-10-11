#pragma once

#include <fom-network/Common.h>
#include <fom-network/FOMNetworkTypes.h>
#include <fom-network/enums/PlayerAttribute.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct PlayerAttributesModel {
  uint32_t attributes[Enums::NUM_ATTRIBUTES];
};
#pragma pack(pop)

ASSERT_BLITTABLE(PlayerAttributesModel);

}  // namespace Packet
}  // namespace FOMNetwork
