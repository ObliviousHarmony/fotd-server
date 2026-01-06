#pragma once

#include <fom-network/Interop.h>
#include <fom-network/enums/PacketIdentifier.h>

#include "packets/PacketSerializers.h"

namespace FOMNetwork {

/**
 * Handles packet data serialization and deserialization based on packet ID.
 */
class FOMDataSerializer {
 public:
  /**
   * Gets the number of packet types the serializer can handle.
   */
  static size_t GetPacketCount();

  /**
   * Fetches the size of a packet based on its ID.
   *
   * @param id The packet ID to fetch the size for.
   */
  static int GetPacketSize(Enum::PacketIdentifier id);

  /**
   * Writes packet data into a bitstream buffer.
   *
   * @param bs The bitstream to write to.
   * @param id The packet ID to write.
   * @param data A buffer containing the packet data to write.
   */
  static bool Write(RakNet::BitStream& bs, const Enum::PacketIdentifier id,
                    const uint8_t* data);

  /**
   * Reads a bitstream buffer into packet data.
   *
   * @param bs The bitstream to read from.
   * @param id The packet ID to read.
   * @param data A buffer to read the packet data into.
   */
  static bool Read(RakNet::BitStream& bs, const Enum::PacketIdentifier id,
                   uint8_t* data);

 private:
  /**
   * Fetches the writer for a given packet ID.
   *
   * @param id The packet ID to fetch the writer for.
   */
  static const IWriter* GetWriter(Enum::PacketIdentifier id);

  /**
   * Fetches the reader for a given packet ID.
   *
   * @param id The packet ID to fetch the reader for.
   */
  static const IReader* GetReader(Enum::PacketIdentifier id);
};

}  // namespace FOMNetwork
