#pragma once

#include <fom-network/Interop.h>
#include <fom-network/types/Position.h>

namespace FOMNetwork {
namespace Type {

#pragma pack(push, 1)
struct PositionRotation {
  Position pos;
  uint16_t rot;
};
#pragma pack(pop)

ASSERT_BLITTABLE(PositionRotation);

}  // namespace Type
}  // namespace FOMNetwork
