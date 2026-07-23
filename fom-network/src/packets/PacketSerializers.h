#pragma once

#include "../BaseSerializer.h"

namespace FOMNetwork {

template <typename Derived, typename PacketType>
class PacketSerializer : public BaseSerializer, public IWriter, public IReader {
 public:
  static Derived& GetInstance() {
    static Derived s;
    return s;
  }

  void WriteRaw(RakNet::BitStream& bs,
                const uint8_t* dataBuffer) const override {
    Write(bs, reinterpret_cast<const PacketType*>(dataBuffer));
  }

  bool ReadRaw(RakNet::BitStream& bs, uint8_t* dataBuffer) const override {
    return Read(bs, reinterpret_cast<PacketType*>(dataBuffer));
  }

  virtual void Write(RakNet::BitStream& bs, const PacketType* data) const = 0;
  virtual bool Read(RakNet::BitStream& bs, PacketType* data) const = 0;
};

template <typename Derived, typename PacketType>
class PacketReader : public BaseSerializer, public IReader {
 public:
  static Derived& GetInstance() {
    static Derived s;
    return s;
  }

  bool ReadRaw(RakNet::BitStream& bs, uint8_t* dataBuffer) const override {
    return Read(bs, reinterpret_cast<PacketType*>(dataBuffer));
  }

  virtual bool Read(RakNet::BitStream& bs, PacketType* data) const = 0;
};

template <typename Derived, typename PacketType>
class PacketWriter : public BaseSerializer, public IWriter {
 public:
  static Derived& GetInstance() {
    static Derived s;
    return s;
  }

  void WriteRaw(RakNet::BitStream& bs,
                const uint8_t* dataBuffer) const override {
    Write(bs, reinterpret_cast<const PacketType*>(dataBuffer));
  }

  virtual void Write(RakNet::BitStream& bs, const PacketType* data) const = 0;
};

class EmptyPacketSerializer : public IWriter, public IReader {
 public:
  static EmptyPacketSerializer& GetInstance() {
    static EmptyPacketSerializer s;
    return s;
  }
  void WriteRaw(RakNet::BitStream& bs, const uint8_t* data) const override {}
  bool ReadRaw(RakNet::BitStream& bs, uint8_t* dataBuffer) const override {
    return true;
  }
};

/**
 * --------------------------------------------------
 * Packet Serializer Macros
 *
 * These macros declare serializer classes using the
 * CRTP base templates. They also forward-declare
 * the packet type, so no packet headers are needed.
 * --------------------------------------------------
 */
#define SERIALIZER_BOTH(TYPE)                                                \
  struct TYPE;                                                               \
  class TYPE##Serializer : public PacketSerializer<TYPE##Serializer, TYPE> { \
   public:                                                                   \
    void Write(RakNet::BitStream& bs, const TYPE* data) const override;      \
    bool Read(RakNet::BitStream& bs, TYPE* data) const override;             \
  };

#define SERIALIZER_WRITE(TYPE)                                           \
  struct TYPE;                                                           \
  class TYPE##Serializer : public PacketWriter<TYPE##Serializer, TYPE> { \
   public:                                                               \
    void Write(RakNet::BitStream& bs, const TYPE* data) const override;  \
  };

#define SERIALIZER_READ(TYPE)                                            \
  struct TYPE;                                                           \
  class TYPE##Serializer : public PacketReader<TYPE##Serializer, TYPE> { \
   public:                                                               \
    bool Read(RakNet::BitStream& bs, TYPE* data) const override;         \
  };

/**
 * Packet Serializer Declarations
 */
SERIALIZER_BOTH(RegisterWorldPacket)
SERIALIZER_READ(LoginRequestPacket)
SERIALIZER_WRITE(LoginRequestReturnPacket)
SERIALIZER_READ(LoginPacket)
SERIALIZER_BOTH(LoginTokenCheckPacket)
SERIALIZER_READ(CheckNamePacket)
SERIALIZER_WRITE(CheckNameReturnPacket)
SERIALIZER_READ(CreateCharacterPacket)
SERIALIZER_WRITE(LoginReturnPacket)
SERIALIZER_READ(WorldLoginPacket)
SERIALIZER_WRITE(WorldLoginReturnPacket)
SERIALIZER_BOTH(PlayerMigrateWorldPacket)
SERIALIZER_BOTH(PlayerWorldReadyPacket)
SERIALIZER_BOTH(PlayerLeavingWorldPacket)
SERIALIZER_READ(RegisterClientPacket)
SERIALIZER_WRITE(RegisterClientReturnPacket)
SERIALIZER_READ(UpdatePacket)
SERIALIZER_WRITE(WorldUpdatePacket)
SERIALIZER_BOTH(ChatPacket)
SERIALIZER_BOTH(MoveItemsPacket);
SERIALIZER_WRITE(WorldObjectsPacket);
SERIALIZER_BOTH(WorldServicePacket);

}  // namespace FOMNetwork
