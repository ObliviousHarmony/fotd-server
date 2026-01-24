#pragma once

#include <fom-network/Interop.h>
#include <fom-network/constants/FactionConstants.h>
#include <fom-network/types/FactionEmblemLayer.h>

namespace FOMNetwork {
namespace Type {

// Wire format: header (uint32_t), then 10x FactionEmblemLayer (bit-prefixed)
#pragma pack(push, 1)
struct FactionEmblem {
  uint32_t header;
  FactionEmblemLayer layers[Constants::NUM_FACTION_EMBLEM_LAYERS];
};
#pragma pack(pop)

ASSERT_BLITTABLE(FactionEmblem);

}  // namespace Type
}  // namespace FOMNetwork
