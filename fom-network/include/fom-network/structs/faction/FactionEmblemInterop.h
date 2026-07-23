#pragma once

#include <fom-network/Interop.h>
#include <fom-network/constants/FactionConstants.h>
#include <fom-network/structs/faction/FactionEmblemLayerInterop.h>

namespace FOMNetwork {

#pragma pack(push, 1)
struct FactionEmblemInterop {
  uint32_t staticEmblemId;
  FactionEmblemLayerInterop layers[Constants::NUM_FACTION_EMBLEM_LAYERS];
};
#pragma pack(pop)

ASSERT_BLITTABLE(FactionEmblemInterop);

}  // namespace FOMNetwork
