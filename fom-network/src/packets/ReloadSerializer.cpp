#include <fom-network/packets/Reload.h>

#include <cstring>

#include "PacketSerializers.h"

namespace FOMNetwork {
namespace Packet {

// Capture probe (see WeaponFireSerializer.cpp).
bool ReloadSerializer::Read(RakNet::BitStream& bs, Packet::Reload* data) const {
  int bits = static_cast<int>(bs.GetNumberOfUnreadBits());
  const int cap = static_cast<int>(sizeof(data->data)) * 8;
  if (bits < 0) bits = 0;
  if (bits > cap) bits = cap;

  data->bitCount = static_cast<uint16_t>(bits);
  std::memset(data->data, 0, sizeof(data->data));
  if (bits > 0) bs.ReadBits(data->data, bits, false);

  return true;
}

}  // namespace Packet
}  // namespace FOMNetwork
