#include <fom-network/FOMDataSerializer.h>

#include <unordered_map>

namespace FOMNetwork {

/**
 * We need to initialize the map with all of the serializers we want to be able
 * to use.
 */
static std::unordered_map<uint32_t, IWriter*> writerMap = {
    {ID_LOGIN_REQUEST_RETURN, &LoginRequestReturnSerializer::GetInstance()},
    {ID_LOGIN_RETURN, &LoginReturnSerializer::GetInstance()},
    {ID_CHECK_NAME_RETURN, &CheckNameReturnSerializer::GetInstance()},
};

static std::unordered_map<uint32_t, IReader*> readerMap = {
    {ID_LOGIN_REQUEST, &LoginRequestSerializer::GetInstance()},
    {ID_LOGIN, &LoginSerializer::GetInstance()},
    {ID_CHECK_NAME, &CheckNameSerializer::GetInstance()},
    {ID_CREATE_CHARACTER, &CreateCharacterSerializer::GetInstance()},
};

bool FOMDataSerializer::Write(RakNet::BitStream& bs, const PacketIdentifier id,
                              const FOMDataUnion& data) {
  if (ShouldForwardRakNetPacket(id)) {
    return true;
  }

  const auto* writer = GetWriter(id);
  if (!writer) {
    return false;
  }

  // Make sure to catch any serialization error so that the
  // library does not crash the consuming application.
  try {
    writer->Write(bs, data);
    return true;
  } catch (const std::exception& e) {
    return false;
  }
}

FOMDataUnion FOMDataSerializer::Read(RakNet::BitStream& bs,
                                     const PacketIdentifier id) {
  if (ShouldForwardRakNetPacket(id)) {
    return FOMDataUnion{};
  }

  const auto* reader = GetReader(id);
  if (!reader) {
    throw ReadError(Packet::ReadPacketError{
        id, Packet::ReadPacketErrorCode::ERROR_UNHANDLED_PACKET_ID});
  }

  // Make sure to catch any deserialization errors so that
  // the library does not crash the consuming application.
  try {
    return reader->Read(bs);
  } catch (const std::exception& e) {
    throw ReadError(Packet::ReadPacketError{
        id, Packet::ReadPacketErrorCode::ERROR_READ});
  }
}

const IWriter* FOMDataSerializer::GetWriter(PacketIdentifier id) {
  auto it = writerMap.find(id);
  if (it == writerMap.end()) {
    return NULL;
  }
  return it->second;
}

const IReader* FOMDataSerializer::GetReader(PacketIdentifier id) {
  auto it = readerMap.find(id);
  if (it == readerMap.end()) {
    return NULL;
  }
  return it->second;
}

}  // namespace FOMNetwork
