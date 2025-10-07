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
  /**
   * The sizes of all of the packet structures that the serializer can handle.
   */
  static const std::unordered_map<uint8_t, size_t> PacketSizes;

  /**
   * Fetches the size of a packet based on its ID.
   *
   * @param id The packet ID to fetch the size for.
   */
  static int GetPacketSize(PacketIdentifier id);

  /**
   * Writes packet data into a bitstream buffer.
   *
   * @param bs The bitstream to write to.
   * @param id The packet ID to write.
   * @param data A buffer containing the packet data to write.
   */
  static bool Write(RakNet::BitStream& bs, const PacketIdentifier id,
                    const uint8_t* data);

  /**
   * Reads a bitstream buffer into packet data.
   *
   * @param bs The bitstream to read from.
   * @param id The packet ID to read.
   * @param data A buffer to read the packet data into.
   */
  static bool Read(RakNet::BitStream& bs, const PacketIdentifier id,
                   uint8_t* data);

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
};

}  // namespace FOMNetwork
