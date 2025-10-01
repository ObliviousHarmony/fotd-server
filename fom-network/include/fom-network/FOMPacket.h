#pragma once

#include <fom-network/PacketIdentifier.h>
#include <fom-network/models/NetworkAddress.h>
#include <fom-network/packets/CheckName.h>
#include <fom-network/packets/CheckNameReturn.h>
#include <fom-network/packets/CreateCharacter.h>
#include <fom-network/packets/Login.h>
#include <fom-network/packets/LoginRequest.h>
#include <fom-network/packets/LoginRequestReturn.h>
#include <fom-network/packets/LoginReturn.h>
#include <fom-network/packets/ReadPacketError.h>
#include <fom-network/packets/RegisterWorld.h>

namespace FOMNetwork {

#pragma pack(push, 1)
struct FOMDataUnion {
  union {
    Packet::ReadPacketError readError;
    Packet::LoginRequest loginRequest;
    Packet::LoginRequestReturn loginRequestReturn;
    Packet::Login login;
    Packet::LoginReturn loginReturn;
    Packet::CheckName checkName;
    Packet::CheckNameReturn checkNameReturn;
    Packet::CreateCharacter createCharacter;
    Packet::RegisterWorld registerWorld;
  };
};
#pragma pack(pop)

#pragma pack(push, 1)
struct FOMPacket {
  PacketIdentifier ID;
  NetworkAddress sender;
  FOMDataUnion data;
};
#pragma pack(pop)

}  // namespace FOMNetwork
