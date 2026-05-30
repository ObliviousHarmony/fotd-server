#pragma once

#include <fom-network/Interop.h>
#include <fom-network/constants/FactionConstants.h>
#include <fom-network/types/FactionEmblemLayer.h>

namespace FOMNetwork {
namespace Type {

#pragma pack(push, 1)
struct FactionEmblem {
  uint32_t staticEmblemId;
  FactionEmblemLayer layers[Constants::NUM_FACTION_EMBLEM_LAYERS];
};
#pragma pack(pop)

ASSERT_BLITTABLE(FactionEmblem);

}  // namespace Type
}  // namespace FOMNetwork
