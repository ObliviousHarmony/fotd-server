#include <fom-network/FOMDataSerializer.h>
#include <fom-network/PacketAPI.h>

#include <vector>

using FOMNetwork::FOMPacket;
using FOMNetwork::FOMDataSerializer;

/**
 * The maximum number of packets that can be received each tick.
 * Since we buffer the packets to return to the consumer, we
 * don't want to stall the thread permanently when there are
 * more packets being received than pulled off the queue.
 */
namespace FOM {
constexpr int32_t MaxBufferedPackets = 256;
}

ReceivedPackets FOMNetwork_ReceivePackets(RakPeerInterface* peer) {
  if (!peer) {
    return {NULL, 0};
  }

  // Make sure to re-use the same buffer since we are just using
  // it to aggregate the pointers before copying them to return.
  static std::vector<Packet*> receiveBuffer;
  receiveBuffer.clear();
  receiveBuffer.reserve(FOM::MaxBufferedPackets);

  // Limit the number of packets
  while (receiveBuffer.size() < FOM::MaxBufferedPackets) {
    Packet* packet = peer->Receive();
    if (!packet) {
      break;
    }

    receiveBuffer.push_back(packet);
  }

  ReceivedPackets received{};

  // We're going to pass an array of packets back to the consumer.
  // The array and the packets contained must all be deallocated
  // in FOMNetwork_ProcessPackets so that we don't leak memory!
  received.count = receiveBuffer.size();
  if (received.count > 0) {
    received.packets = new Packet*[receiveBuffer.size()];
    std::copy(receiveBuffer.begin(), receiveBuffer.end(), received.packets);
  }

  return received;
}

int32_t FOMNetwork_ProcessPackets(RakPeerInterface* peer,
                                  const ReceivedPackets received,
                                  FOMPacket* packetBuffer,
                                  int32_t packetBufferLen) {
  if (!peer || !received.packets || received.count == 0) {
    return 0;
  }

  if (!packetBuffer || packetBufferLen != received.count) {
    return -1;
  }

  for (int32_t i = 0; i < received.count; i++) {
    Packet* p = received.packets[i];
    if (!p) {
      continue;
    }

    RakNet::BitStream bs(p->data, p->length, false);

    // Deserialize the bitstream into a packet structure that can be returned to
    // the consumer.
    FOMPacket& fp =
        packetBuffer[i];  // Don't allocate, just use the provided buffer.
    bs.Read(fp.ID);       // First byte is always the packet ID.

    try {
      fp.data = FOMDataSerializer::Read(bs, fp.ID);
    } catch (const FOMDataSerializer::ReadError& e) {
      // Make sure that read errors are communicated to the consumer.
      fp.ID = FOMNetwork::ID_FOM_PACKET_READ_ERROR;
      fp.data.readError = e.readError;
    }

    fp.sender.binaryAddress = p->systemAddress.binaryAddress;
    fp.sender.port = p->systemAddress.port;

    // Release the packet back to RakNet.
    peer->DeallocatePacket(const_cast<Packet*>(p));
  }

  // Now that we're done we don't need the receive packet buffer anymore.
  delete[] received.packets;

  return 0;
}

int32_t FOMNetwork_Send(RakPeerInterface* peer, const SendPacket* packets,
                        int32_t count) {
  if (!peer || !packets || count == 0) {
    return -1;
  }

  int32_t packetsSent = 0;
  for (int32_t i = 0; i < count; i++) {
    const SendPacket& s = packets[i];

    SystemAddress address = UNASSIGNED_SYSTEM_ADDRESS;
    if (s.networkAddress.binaryAddress != 0) {
      address.binaryAddress = s.networkAddress.binaryAddress;
      address.port = s.networkAddress.port;
    }

    RakNet::BitStream bs;
    bs.Write(s.id);
    if (!FOMDataSerializer::Write(bs, s.id, s.data)) {
      continue;
    }

    packetsSent++;
    if (s.broadcast) {
      peer->Send(&bs, (PacketPriority)s.priority,
                 (PacketReliability)s.reliability, s.orderingChannel, address,
                 s.broadcast);
    } else {
      peer->Send(&bs, (PacketPriority)s.priority,
                 (PacketReliability)s.reliability, s.orderingChannel, address,
                 s.broadcast);
    }
  }

  return packetsSent;
}
