#pragma once

#include <fom-network/Common.h>

namespace FOMNetwork {
namespace Enums {

enum MedicalTreatment : uint8_t {
  INVALID_TREATMENT = 0,
  TREATMENT_NANOSURGERY = 1,
  TREATMENT_PASSIVE = 2,
  TREATMENT_STAMINA = 3,
};

}  // namespace Enums
}  // namespace FOMNetwork
