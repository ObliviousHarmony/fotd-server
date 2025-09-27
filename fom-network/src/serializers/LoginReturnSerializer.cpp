#include <fom-network/PacketSerializers.h>

void LoginReturnSerializer::WriteData(
    RakNet::BitStream& bs, const FOMPacket::LoginReturn& data) const {
  bs.WriteCompressed(data.status);
  bs.WriteCompressed(data.playerID);
  bs.WriteCompressed(data.accountType);
  bs.WriteCompressed(data.isVolunteer);

  if (data.isVolunteer)
    bs.Write1();
  else
    bs.Write0();

  bs.WriteCompressed(data.ClientVersion);

  if (data.isBanned)
    bs.Write1();
  else
    bs.Write0();
}
