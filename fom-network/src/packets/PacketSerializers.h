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
  namespace Packet {                                                         \
  struct TYPE;                                                               \
  class TYPE##Serializer : public PacketSerializer<TYPE##Serializer, TYPE> { \
   public:                                                                   \
    void Write(RakNet::BitStream& bs, const TYPE* data) const override;      \
    bool Read(RakNet::BitStream& bs, TYPE* data) const override;             \
  };                                                                         \
  }

#define SERIALIZER_WRITE(TYPE)                                           \
  namespace Packet {                                                     \
  struct TYPE;                                                           \
  class TYPE##Serializer : public PacketWriter<TYPE##Serializer, TYPE> { \
   public:                                                               \
    void Write(RakNet::BitStream& bs, const TYPE* data) const override;  \
  };                                                                     \
  }

#define SERIALIZER_READ(TYPE)                                            \
  namespace Packet {                                                     \
  struct TYPE;                                                           \
  class TYPE##Serializer : public PacketReader<TYPE##Serializer, TYPE> { \
   public:                                                               \
    bool Read(RakNet::BitStream& bs, TYPE* data) const override;         \
  };                                                                     \
  }

/**
 * Packet Serializer Declarations
 */
SERIALIZER_BOTH(RegisterWorld)
SERIALIZER_READ(LoginRequest)
SERIALIZER_WRITE(LoginRequestReturn)
SERIALIZER_READ(Login)
SERIALIZER_BOTH(LoginTokenCheck)
SERIALIZER_READ(CheckName)
SERIALIZER_WRITE(CheckNameReturn)
SERIALIZER_READ(CreateCharacter)
SERIALIZER_WRITE(LoginReturn)
SERIALIZER_READ(WorldLogin)
SERIALIZER_WRITE(WorldLoginReturn)
SERIALIZER_BOTH(PlayerMigrateWorld)
SERIALIZER_BOTH(PlayerWorldReady)
SERIALIZER_BOTH(PlayerLeavingWorld)
SERIALIZER_READ(RegisterClient)
SERIALIZER_WRITE(RegisterClientReturn)
SERIALIZER_READ(Update)
SERIALIZER_WRITE(WorldUpdate)
SERIALIZER_BOTH(Chat)

}  // namespace FOMNetwork
