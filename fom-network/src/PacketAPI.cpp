#include <fom-network/FOMDataSerializer.h>
#include <fom-network/PacketAPI.h>
#include <fom-network/packets/PacketIdentifier.h>
#include <raknet/GetTime.h>

#include <vector>

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
FOMNetwork::PacketIdentifier GetPacketIdentifier(Packet* p) {
  if (p->data[0] == ID_TIMESTAMP)
    return (FOMNetwork::PacketIdentifier)
        p->data[sizeof(uint8_t) + sizeof(RakNetTime)];
  else
    return (FOMNetwork::PacketIdentifier)p->data[0];
}

ReceivedPackets FOMNetwork_ReceivePackets(RakPeerInterface* peer) {
  ReceivedPackets received{};

  if (!peer) {
    return received;
  }

  // Only buffer up to MaxBufferedPackets packets at a time.
  int count = 0;
  Packet* receiveBuffer[FOM::MaxBufferedPackets];
  while (count < FOM::MaxBufferedPackets) {
    Packet* p = peer->Receive();
    if (!p) {
      break;
    }

    // We can only handle packets that we know about.
    auto packetID = GetPacketIdentifier(p);
    int packetSize = FOMDataSerializer::GetPacketSize(packetID);
    if (packetSize <= 0) {
      peer->DeallocatePacket(const_cast<Packet*>(p));
      continue;
    }

    receiveBuffer[count++] = p;
  }

  // We're going to pass an array of packets back to the consumer.
  // The array and the packets contained must all be deallocated
  // in FOMNetwork_ProcessPackets so that we don't leak memory!
  received.count = count;
  if (received.count > 0) {
    received.packets = new Packet*[count];
    received.senders = new FOMNetwork::NetworkAddress[count];
    received.identifiers = new FOMNetwork::PacketIdentifier[count];
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

int32_t FOMNetwork_ProcessPackets(RakPeerInterface* peer,
                                  const ReceivedPackets received,
                                  uint8_t* packetBuffer,
                                  int32_t packetBufferLen) {
  if (!peer || !received.packets || received.count == 0) {
    return 0;
  }

  if (!packetBuffer) {
    return -1;
  }

  int packetBufferOffset = 0;
  int ret = 0;
  for (int32_t i = 0; i < received.count; i++) {
    Packet* p = received.packets[i];
    if (!p) {
      continue;
    }

    // Don't try to deserialize packets that we don't know about.
    FOMNetwork::PacketIdentifier packetID = received.identifiers[i];
    int packetSize = FOMDataSerializer::GetPacketSize(packetID);
    if (packetSize <= 0) {
      ret = -1;
      peer->DeallocatePacket(const_cast<Packet*>(p));
      continue;
    }

    // Make sure that the buffer can hold this packet.
    if (packetBufferOffset + packetSize > packetBufferLen) {
      ret = -2;
      peer->DeallocatePacket(const_cast<Packet*>(p));
      continue;
    }

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
      ret = -1;
      peer->DeallocatePacket(const_cast<Packet*>(p));
      continue;
    }

    // Read the packet bitstream into our packet's buffer.
    FOMDataSerializer::Read(bs, packetID, &packetBuffer[packetBufferOffset]);
    packetBufferOffset += packetSize;

    // Release the packet back to RakNet.
    peer->DeallocatePacket(const_cast<Packet*>(p));
  }

  // Now that we're done we need to free the memory from the receive structure.
  delete[] received.packets;
  delete[] received.senders;
  delete[] received.identifiers;

  return ret;
}

int32_t FOMNetwork_Send(RakPeerInterface* peer, const SendPacket* packets,
                        int32_t count) {
  if (!peer || !packets || count == 0) {
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
      const FOMNetwork::NetworkAddress& addr = s.networkAddresses[j];

      bool sent =
          peer->Send(&bs, (PacketPriority)s.priority,
                     (PacketReliability)s.reliability, s.orderingChannel,
                     SystemAddress(addr.binaryAddress, addr.port), s.broadcast);
      if (sent) {
        packetsSent++;
      }
    }
  }

  return packetsSent;
}
