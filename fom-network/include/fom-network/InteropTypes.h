#pragma once

#include <fom-network/constants/BufferSizes.h>

#include <cstdint>

namespace FOMNetwork {

enum PacketPriority : uint8_t {
  SYSTEM_PRIORITY,
  HIGH_PRIORITY,
  MEDIUM_PRIORITY,
  LOW_PRIORITY,
  NUMBER_OF_PRIORITIES
};

enum PacketReliability : uint8_t {
  UNRELIABLE,
  UNRELIABLE_SEQUENCED,
  RELIABLE,
  RELIABLE_ORDERED,
  RELIABLE_SEQUENCED,
  NUMBER_OF_RELIABILITIES
};

}  // namespace FOMNetwork
