#pragma once

#include <fom-network/Interop.h>
#include <fom-network/enums/ApartmentType.h>
#include <fom-network/enums/WorldID.h>
#include <fom-network/types/ItemList.h>

namespace FOMNetwork {
namespace Type {

#pragma pack(push, 1)
struct Apartment {
  uint32_t id;
  Enum::ApartmentType type;
  uint32_t ownerPlayerID;
  uint32_t ownerFactionID;
  uint8_t isOpen;
  uint8_t ownerName[BufferSizes::PLAYER_NAME];
  uint8_t entryCode[8];
  ItemList storageItems;
  uint8_t isPublic;
  uint32_t entryPrice;
  uint8_t publicName[24];
  uint8_t publicDescription[512];
  uint8_t isDefault;
  uint8_t isFeatured;
  uint32_t occupants;
};
#pragma pack(pop)

ASSERT_BLITTABLE(Apartment);

}  // namespace Type
}  // namespace FOMNetwork
