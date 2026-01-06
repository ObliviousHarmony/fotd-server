#pragma once

#include <fom-network/InteropTypes.h>

namespace FOMNetwork {
namespace Enum {

/**
 * Status code prefixed to each packet in the buffer.
 * Used to indicate whether deserialization succeeded or failed.
 */
enum SerializationStatus : uint8_t {
  SERIALIZATION_SUCCESS = 0,
  SERIALIZATION_READ_ERROR = 1,
  SERIALIZATION_UNHANDLED_PACKET = 2,
};

}  // namespace Enum
}  // namespace FOMNetwork
