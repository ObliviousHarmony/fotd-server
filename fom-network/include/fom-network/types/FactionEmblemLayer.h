#pragma once

#include <fom-network/Interop.h>

namespace FOMNetwork {
namespace Type {

// Wire format: 1-bit presence flag, then if present:
// shape (uint16_t), offsetX (int8_t), offsetY (int8_t),
// scaleWidth (7 bits), scaleHeight (7 bits), rotation (9 bits),
// red (uint8_t), green (uint8_t), blue (uint8_t)
#pragma pack(push, 1)
struct FactionEmblemLayer {
  uint16_t shape;
  int8_t offsetX;
  int8_t offsetY;
  uint8_t scaleWidth;
  uint8_t scaleHeight;
  uint16_t rotation;
  uint8_t red;
  uint8_t green;
  uint8_t blue;
};
#pragma pack(pop)

ASSERT_BLITTABLE(FactionEmblemLayer);

}  // namespace Type
}  // namespace FOMNetwork
