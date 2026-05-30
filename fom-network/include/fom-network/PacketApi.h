#pragma once

#include <fom-network/Interop.h>
#include <fom-network/enums/PacketIdentifier.h>
#include <fom-network/types/NetworkAddress.h>

/**
 * Container for passing a buffer of received packets around.
 *
 * @note This structure MUST only contain C# blittable types.
 */
#pragma pack(push, 1)
struct ReceivedPackets {
  /**
   * The number of packets in the buffer.
   */
  uint8_t count;

  /**
   * A buffer of packets that have been received and need to be deserialized and
   * released.
   */
  FOMNetworkRawPacket** packets;

  /**
   * The senders for each of the received packets.
   */
  FOMNetwork::Type::NetworkAddress* senders;

  /**
   * The packet identifiers for each of the received packets.
   */
  FOMNetwork::Enum::PacketIdentifier* identifiers;
};
#pragma pack(pop)

ASSERT_BLITTABLE(ReceivedPackets)

/**
 * Container for passing packets to be sent around.
 *
 * @note This structure MUST only contain C# blittable types.
 */
#pragma pack(push, 1)
struct SendPacket {
  /**
   * The identifier for the packet being sent.
   */
  FOMNetwork::Enum::PacketIdentifier id;

  /**
   * A pointer to the memory containing the packet data.
   */
  uint8_t* data;

  /**
   * The number of network addresses in the packet.
   */
  int32_t numNetworkAddresses;

  /**
   * An array of network addresses to either send the packet to or
   * exclude from a broadcast.
   */
  FOMNetwork::Type::NetworkAddress* networkAddresses;

  /**
   * The priority of the packet to be sent to the networking library.
   */
  uint8_t priority;

  /**
   * The reliability of the packet to be sent to the networking library.
   */
  uint8_t reliability;

  /**
   * The ordering channel for the packet to be sent to the networking library.
   */
  uint8_t orderingChannel;

  /**
   * A boolean indicating whether or not the packet should be a broadcast.
   */
  uint8_t broadcast;
};
#pragma pack(pop)

ASSERT_BLITTABLE(SendPacket)

#ifdef __cplusplus
extern "C" {
#endif

/**
 * Polls the network interface for packets and returns them in a buffer.
 *
 * Receiving packets from the API is done in two stages. First, the caller
 * receives a buffer of packets that need to be processed. The caller should
 * then allocate memory for a buffer that the library will use to store the
 * deserialized packet structures. Note that the returned packets will not be
 * automatically deallocated and must be freed by processing the packets.
 *
 * @param peer A pointer to the network interface.
 * @return A structure containing a buffer of received packets and the number of
 * packets in the buffer.
 */
FOM_API ReceivedPackets FOMNetwork_ReceivePackets(FOMNetworkPeer* peer);

/**
 * Uses the received packets to fill a buffer with deserialized packet
 * structures.
 *
 * The caller should provide a buffer sized to hold all packets. Each packet
 * slot in the buffer uses the format: [status byte][packet data]. The buffer
 * size should be calculated as: sum(1 + sizeof(packet)) for each packet.
 *
 * The status byte indicates deserialization result:
 * - SERIALIZATION_SUCCESS (0): Packet data is valid
 * - SERIALIZATION_READ_ERROR (1): Deserialization failed
 * - SERIALIZATION_UNHANDLED_PACKET (2): No reader for packet Id
 *
 * @param peer A pointer to the network interface.
 * @param received A structure containing a buffer of received packets and the
 * number of packets in the buffer.
 * @param packetBuffer A buffer to be filled with status bytes and deserialized
 * packet structures.
 * @param packetBufferLen The size of the packet buffer in bytes.
 * @return int32_t The status code.
 * @retval 0 Success.
 * @retval -1 A packet Id was received that could not be deserialized.
 * @retval -2 The packetBufferLen is not able to hold all of the received
 * packets.
 * @retval -3 There was a mismatch between a packet's Id and the Id provided in
 * the received argument.
 */
FOM_API int32_t FOMNetwork_ProcessPackets(FOMNetworkPeer* peer,
                                          const ReceivedPackets received,
                                          uint8_t* packetBuffer,
                                          int32_t packetBufferLen);

/**
 * Sends a buffer of packet structures through the network interface.
 *
 * @param peer A pointer to the network interface.
 * @param packets A buffer of packet structures to serialize and send.
 * @param count The number of packets in the buffer.
 * @return int32_t The number of packets sent or a status code on error.
 * @retval >=0 The number of packets sent.
 * @retval -1 No packets were provided to send.
 * @retval -2 A broadcast packet specified more than one network address.
 */
FOM_API int32_t FOMNetwork_Send(FOMNetworkPeer* peer, const SendPacket* packets,
                                int32_t count);

#ifdef __cplusplus
}
#endif
