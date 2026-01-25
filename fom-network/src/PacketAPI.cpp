#include <fom-network/PacketAPI.h>
#include <fom-network/enums/PacketIdentifier.h>
#include <fom-network/enums/SerializationStatus.h>

#include <cstring>
#include <vector>

#include "FOMDataSerializer.h"

using FOMNetwork::FOMDataSerializer;

/**
 * The maximum number of packets that can be received each tick.
 * Since we buffer the packets to return to the consumer, we
 * don't want to stall the thread permanently when there are
 * more packets being received than pulled off the queue.
 */
namespace FOM {
constexpr int32_t MaxBufferedPackets = 128;
}

/**
 * Fetches the packet identifier from a packet, accounting for
 * the timestamp byte if present.
 */
FOMNetwork::Enum::PacketIdentifier GetPacketIdentifier(Packet* p) {
  if (p->data[0] == ID_TIMESTAMP)
    return (FOMNetwork::Enum::PacketIdentifier)
        p->data[sizeof(uint8_t) + sizeof(RakNetTime)];
  else
    return (FOMNetwork::Enum::PacketIdentifier)p->data[0];
}

ReceivedPackets FOMNetwork_ReceivePackets(FOMNetworkPeer* peer) {
  ReceivedPackets received{};

  auto rakPeer = static_cast<RakPeerInterface*>(peer);
  if (!rakPeer) {
    return received;
  }

  // Only buffer up to MaxBufferedPackets packets at a time.
  int count = 0;
  Packet* receiveBuffer[FOM::MaxBufferedPackets];
  while (count < FOM::MaxBufferedPackets) {
    Packet* p = rakPeer->Receive();
    if (!p) {
      break;
    }

    // We can only handle packets that we know about.
    auto packetID = GetPacketIdentifier(p);
    int packetSize = FOMDataSerializer::GetPacketSize(packetID);
    if (packetSize <= 0) {
      rakPeer->DeallocatePacket(p);
      continue;
    }

    receiveBuffer[count++] = p;
  }

  // We're going to pass an array of packets back to the consumer.
  // The array and the packets contained must all be deallocated
  // in FOMNetwork_ProcessPackets so that we don't leak memory!
  received.count = count;
  if (received.count > 0) {
    received.packets = new void*[count];
    received.senders = new FOMNetwork::Type::NetworkAddress[count];
    received.identifiers = new FOMNetwork::Enum::PacketIdentifier[count];
    for (int i = 0; i < count; i++) {
      Packet* p = receiveBuffer[i];

      received.packets[i] = p;
      received.senders[i].binaryAddress = p->systemAddress.binaryAddress;
      received.senders[i].port = p->systemAddress.port;
      received.identifiers[i] = GetPacketIdentifier(p);
    }
  }

  return received;
}

int32_t FOMNetwork_ProcessPackets(FOMNetworkPeer* peer,
                                  const ReceivedPackets received,
                                  uint8_t* packetBuffer,
                                  int32_t packetBufferLen) {
  auto rakPeer = static_cast<RakPeerInterface*>(peer);
  if (!rakPeer || !received.packets || received.count == 0) {
    delete[] received.packets;
    delete[] received.senders;
    delete[] received.identifiers;
    return 0;
  }

  // Zero the buffer so that any unread fields don't contain garbage.
  if (packetBuffer && packetBufferLen > 0)
    memset(packetBuffer, 0, packetBufferLen);

  int packetBufferOffset = 0;
  int ret = 0;
  for (int32_t i = 0; i < received.count; i++) {
    Packet* p = static_cast<Packet*>(received.packets[i]);
    if (!p) {
      continue;
    }

    // Don't try to deserialize packets that we don't know about.
    FOMNetwork::Enum::PacketIdentifier packetID = received.identifiers[i];
    int packetSize = FOMDataSerializer::GetPacketSize(packetID);
    if (packetSize <= 0) {
      ret = -1;
      rakPeer->DeallocatePacket(p);
      continue;
    }

    // Each slot is 1 byte status + packet data.
    int slotSize = 1 + packetSize;

    // Make sure that the buffer can hold this packet.
    if (!packetBuffer || packetBufferOffset + slotSize > packetBufferLen) {
      ret = -2;
      rakPeer->DeallocatePacket(p);
      continue;
    }

    // Get pointers to status byte and packet data within the buffer.
    uint8_t* statusByte = &packetBuffer[packetBufferOffset];
    uint8_t* packetData = &packetBuffer[packetBufferOffset + 1];

    RakNet::BitStream bs(p->data, p->length, false);

    // Skip the packet ID and rely on what the consumer gave us.
    // The buffer was size based on those IDs and using a
    // different one will lead to memory corruption.
    uint8_t rawPacketID;
    bs.Read(rawPacketID);
    if (rawPacketID == ID_TIMESTAMP) {
      // Skip the timestamp too if one is present.
      bs.IgnoreBytes(sizeof(RakNetTime));
      bs.Read(rawPacketID);
    }

    if (rawPacketID != packetID) {
      // This should never happen, but if it does we don't
      // want to try to read the packet.
      *statusByte = FOMNetwork::Enum::SERIALIZATION_UNHANDLED_PACKET;
      ret = -3;
      packetBufferOffset += slotSize;
      rakPeer->DeallocatePacket(p);
      continue;
    }

    // Read the packet bitstream into our packet's buffer.
    bool readSuccess = FOMDataSerializer::Read(bs, packetID, packetData);
    *statusByte = readSuccess ? FOMNetwork::Enum::SERIALIZATION_SUCCESS
                              : FOMNetwork::Enum::SERIALIZATION_READ_ERROR;

    packetBufferOffset += slotSize;

    // Release the packet back to RakNet.
    rakPeer->DeallocatePacket(p);
  }

  // Now that we're done we need to free the memory from the receive structure.
  delete[] received.packets;
  delete[] received.senders;
  delete[] received.identifiers;

  return ret;
}

int32_t FOMNetwork_Send(FOMNetworkPeer* peer, const SendPacket* packets,
                        int32_t count) {
  auto rakPeer = static_cast<RakPeerInterface*>(peer);
  if (!rakPeer || !packets || count == 0) {
    return -1;
  }

  int32_t packetsSent = 0;
  for (int32_t i = 0; i < count; i++) {
    const SendPacket& s = packets[i];

    // Broadcasts should only have a single exclusion address.
    if (s.broadcast && s.numNetworkAddresses > 1) {
      return -2;
    }

    RakNet::BitStream bs;

    // All packets include a timestamp
    bs.Write((uint8_t)ID_TIMESTAMP);
    bs.Write(RakNet::GetTime());

    bs.Write(s.id);
    if (!FOMDataSerializer::Write(bs, s.id, s.data)) {
      continue;
    }

    for (int32_t j = 0; j < s.numNetworkAddresses; j++) {
      const FOMNetwork::Type::NetworkAddress& addr = s.networkAddresses[j];

      bool sent = rakPeer->Send(
          &bs, (PacketPriority)s.priority, (PacketReliability)s.reliability,
          s.orderingChannel, SystemAddress(addr.binaryAddress, addr.port),
          s.broadcast);
      if (sent) {
        packetsSent++;
      }
    }
  }

  return packetsSent;
}
