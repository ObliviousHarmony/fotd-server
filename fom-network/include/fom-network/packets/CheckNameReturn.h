#pragma once

#include <fom-network/Interop.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct CheckNameReturn {
  PlayerID_t ownerPlayerID;
};
#pragma pack(pop)

ASSERT_BLITTABLE(CheckNameReturn);

}  // namespace Packet
}  // namespace FOMNetwork
