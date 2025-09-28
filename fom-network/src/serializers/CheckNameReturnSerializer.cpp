#include <fom-network/PacketSerializers.h>

void CheckNameReturnSerializer::WriteData(
    RakNet::BitStream& bs, const FOMPacket::CheckNameReturn& data) const {
  bs.WriteCompressed(data.existingPlayerID);
}
