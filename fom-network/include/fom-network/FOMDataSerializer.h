#pragma once

#include <fom-network/FOMNetworkExport.h>
#include <fom-network/packets/FOMPacket.h>
#include <fom-network/packets/PacketSerializers.h>

#include <memory>
#include <stdexcept>
#include <unordered_map>

namespace FOMNetwork {

/**
 * Handles packet data serialization and deserialization based on packet ID.
 */
class FOM_API FOMDataSerializer {
 public:
  class ReadError : public std::runtime_error {
   public:
    FOMNetwork::Packet::ReadPacketError readError;

    ReadError(FOMNetwork::Packet::ReadPacketError data)
        : std::runtime_error("An error occurred during reading"),
          readError(data) {}
  };

  /**
   * Writes packet data into a bitstream buffer.
   *
   * @param bs The bitstream to write to.
   * @param id The packet ID to write.
   * @param data The packet data to write.
   */
  static bool Write(RakNet::BitStream& bs, const PacketIdentifier id,
                    const FOMDataUnion& data);

  /**
   * Reads a bitstream buffer into packet data.
   *
   * @param bs The bitstream to read from.
   * @param id The packet ID to read.
   */
  static FOMDataUnion Read(RakNet::BitStream& bs, const PacketIdentifier id);

 private:
  /**
   * Fetches the writer for a given packet ID.
   *
   * @param id The packet ID to fetch the writer for.
   */
  static const IWriter* GetWriter(PacketIdentifier id);

  /**
   * Fetches the reader for a given packet ID.
   *
   * @param id The packet ID to fetch the reader for.
   */
  static const IReader* GetReader(PacketIdentifier id);

  /**
   * Checks to see if the given packet ID is one of the RakNet packets we
   * want to forward the ID of to the consumer.
   *
   * @param id The packet ID to check.
   */
  static const bool ShouldForwardRakNetPacket(PacketIdentifier id) {
    // RakNet client packets are "serialized" but we don't actually need to do
    // anything with the data since we're only forwarding the ID to the
    // consumer.
    switch (id) {
      // Connection request to the server has been accepted.
      case ID_CONNECTION_REQUEST_ACCEPTED:
      // Could not connect to the server.
      case ID_CONNECTION_ATTEMPT_FAILED:
      // Attempted to connect to a server we're already connected to.
      case ID_ALREADY_CONNECTED:
      // Connection has been made successfully.
      case ID_NEW_INCOMING_CONNECTION:
      // Server is not accepting new connections.
      case ID_NO_FREE_INCOMING_CONNECTIONS:
      // The connected system has shut down.
      case ID_DISCONNECTION_NOTIFICATION:
      // The connection has been closed.
      case ID_CONNECTION_LOST:
      // Current RSA public key does not match what the destination expected.
      case ID_RSA_PUBLIC_KEY_MISMATCH:
      // Client is banned by the server.
      case ID_CONNECTION_BANNED:
      // Server has refused the client's passeord.
      case ID_INVALID_PASSWORD:
      // Packet has been tampered with in transit.
      case ID_MODIFIED_PACKET:
        return true;
      default:
        return false;
    }
  }
};

}  // namespace FOMNetwork
