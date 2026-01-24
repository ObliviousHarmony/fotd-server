#pragma once

#include <fom-network/Interop.h>

namespace FOMNetwork {
namespace Type {

#pragma pack(push, 1)
struct Position {
  uint32_t precision;
  int16_t x;
  int16_t y;
  int16_t z;
  uint16_t rot;
};
#pragma pack(pop)

ASSERT_BLITTABLE(Position);

}  // namespace Type
}  // namespace FOMNetwork
