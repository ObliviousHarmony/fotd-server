#pragma once

#include <fom-network/Interop.h>
#include <fom-network/enums/ApartmentType.h>
#include <fom-network/enums/WorldId.h>
#include <fom-network/structs/item/ItemListInterop.h>

namespace FOMNetwork {

#pragma pack(push, 1)
struct ApartmentInterop {
  uint32_t id;
  Enum::ApartmentType type;
  uint32_t ownerPlayerId;
  uint32_t ownerFactionId;
  uint8_t isOpen;
  uint8_t ownerName[BufferSizes::PLAYER_NAME];
  uint8_t entryCode[8];
  ItemListInterop storageItems;
  uint8_t isPublic;
  uint32_t entryPrice;
  uint8_t publicName[24];
  uint8_t publicDescription[512];
  uint8_t isDefault;
  uint8_t isFeatured;
  uint32_t occupants;
};
#pragma pack(pop)

ASSERT_BLITTABLE(ApartmentInterop);

}  // namespace FOMNetwork
