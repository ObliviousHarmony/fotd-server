#pragma once

#include <fom-network/packets/NetworkAddress.h>
#include <fom-network/packets/PacketIdentifier.h>
#include <fom-network/packets/data/CheckName.h>
#include <fom-network/packets/data/CheckNameReturn.h>
#include <fom-network/packets/data/CreateCharacter.h>
#include <fom-network/packets/data/Login.h>
#include <fom-network/packets/data/LoginRequest.h>
#include <fom-network/packets/data/LoginRequestReturn.h>
#include <fom-network/packets/data/LoginReturn.h>
#include <fom-network/packets/data/ReadPacketError.h>
#include <fom-network/packets/data/RegisterWorld.h>
#include <fom-network/packets/data/WorldLogin.h>
#include <fom-network/packets/data/WorldLoginReturn.h>
#include <fom-network/packets/data/WorldOverview.h>
#include <fom-network/packets/data/WorldOverviewReturn.h>
#include <fom-network/packets/data/raknet/AlreadyConnected.h>
#include <fom-network/packets/data/raknet/ConnectionAttemptFailed.h>
#include <fom-network/packets/data/raknet/ConnectionBanned.h>
#include <fom-network/packets/data/raknet/ConnectionLost.h>
#include <fom-network/packets/data/raknet/ConnectionRequestAccepted.h>
#include <fom-network/packets/data/raknet/DisconnectionNotification.h>
#include <fom-network/packets/data/raknet/InvalidPassword.h>
#include <fom-network/packets/data/raknet/ModifiedPacket.h>
#include <fom-network/packets/data/raknet/NewIncomingConnection.h>
#include <fom-network/packets/data/raknet/NoFreeIncomingConnections.h>
#include <fom-network/packets/data/raknet/RSAPublicKeyMismatch.h>

namespace FOMNetwork {

#pragma pack(push, 1)
struct FOMDataUnion {
  union {
    Packet::AlreadyConnected alreadyConnected;
    Packet::ConnectionAttemptFailed connectionAttemptFailed;
    Packet::ConnectionBanned connectionBanned;
    Packet::ConnectionLost connectionLost;
    Packet::ConnectionRequestAccepted connectionRequestAccepted;
    Packet::DisconnectionNotification disconnectionNotification;
    Packet::InvalidPassword invalidPassword;
    Packet::ModifiedPacket modifiedPacket;
    Packet::NewIncomingConnection newIncomingConnection;
    Packet::NoFreeIncomingConnections noFreeIncomingConnections;
    Packet::RSAPublicKeyMismatch rsaPublicKeyMismatch;
    Packet::ReadPacketError readError;
    Packet::LoginRequest loginRequest;
    Packet::LoginRequestReturn loginRequestReturn;
    Packet::Login login;
    Packet::LoginReturn loginReturn;
    Packet::CheckName checkName;
    Packet::CheckNameReturn checkNameReturn;
    Packet::CreateCharacter createCharacter;
    Packet::RegisterWorld registerWorld;
    Packet::WorldOverview worldOverview;
    Packet::WorldOverviewReturn worldOverviewReturn;
    Packet::WorldLogin worldLogin;
    Packet::WorldLoginReturn worldLoginReturn;
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
