#pragma once

#include <fom-network/FOMNetworkExport.h>
#include <fom-network/packets/FOMPacket.h>

#pragma warning(push)
#pragma warning(disable : 26495)

#include <raknet/BitStream.h>
#include <raknet/StringCompressor.h>

#pragma warning(pop)

namespace FOMNetwork {

/**
 * Base interfaces for packet serializers.
 */
struct IWriter {
  virtual ~IWriter() = default;
  virtual void Write(RakNet::BitStream& bs, const FOMDataUnion& data) const = 0;
};

struct IReader {
  virtual ~IReader() = default;
  virtual bool Read(RakNet::BitStream& bs, uint8_t* dataBuffer) const = 0;
};

class BaseSerializer {
 protected:
  BaseSerializer() {
    StringCompressor::AddReference();
    strCompressor = StringCompressor::Instance();
  }

  ~BaseSerializer() { StringCompressor::RemoveReference(); }

  template <typename T>
  void WriteBits(RakNet::BitStream& bs, const T& input,
                 int numberOfBitsToWrite) const {
    bs.WriteBits((uint8_t*)&input, numberOfBitsToWrite);
  }

  template <typename T>
  bool ReadBits(RakNet::BitStream& bs, T& input,
                int numberOfBitsToWrite) const {
    return bs.ReadBits((uint8_t*)&input, numberOfBitsToWrite);
  }

  template <size_t N>
  void WriteRawString(RakNet::BitStream& bs, const uint8_t (&input)[N]) const {
    bs.Write((uint8_t)N);
    bs.WriteBits(input, N * 8);
  }

  template <size_t N>
  bool ReadRawString(RakNet::BitStream& bs, uint8_t (&output)[N]) const {
    uint8_t len;
    if (!bs.Read(len)) return false;
    if (len > N) return false;
    return bs.ReadBits(output, len * 8);
  }

  template <size_t N>
  void EncodeString(RakNet::BitStream& bs, const uint8_t (&input)[N]) const {
    strCompressor->EncodeString((const char*)input, N, &bs);
  }

  template <size_t N>
  bool DecodeString(RakNet::BitStream& bs, uint8_t (&output)[N]) const {
    return strCompressor->DecodeString((char*)output, N, &bs);
  }

 private:
  StringCompressor* strCompressor;
};

class FOM_API EmptyPacketSerializer : public IWriter, public IReader {
 public:
  static EmptyPacketSerializer& GetInstance() {
    static EmptyPacketSerializer s;
    return s;
  }
  void Write(RakNet::BitStream& bs, const FOMDataUnion& d) const override {}
  bool Read(RakNet::BitStream& bs, uint8_t* dataBuffer) const override {
    // There isn't any struct data to read since it's empty.
    return true;
  }
};

/**
 * --------------------------------------------------
 * Packet Serializer Macros
 *
 * In order to eliminate the boilerplate class
 * declarations for serializers, these macros
 * will do the work of declaring the class
 * for you.
 * --------------------------------------------------
 */
#define SERIALIZER_BOTH(TYPE, FIELD)                                          \
  class FOM_API TYPE##Serializer : public BaseSerializer,                     \
                                   public IWriter,                            \
                                   public IReader {                           \
   public:                                                                    \
    static TYPE##Serializer& GetInstance() {                                  \
      static TYPE##Serializer s;                                              \
      return s;                                                               \
    }                                                                         \
    void Write(RakNet::BitStream& bs, const FOMDataUnion& d) const override { \
      WriteData(bs, d.FIELD);                                                 \
    }                                                                         \
    bool Read(RakNet::BitStream& bs, uint8_t* dataBuffer) const override {    \
      FOMNetwork::Packet::TYPE* data =                                        \
          reinterpret_cast<FOMNetwork::Packet::TYPE*>(dataBuffer);            \
      return ReadData(bs, *data);                                             \
    }                                                                         \
    void WriteData(RakNet::BitStream& bs,                                     \
                   const FOMNetwork::Packet::TYPE& v) const;                  \
    bool ReadData(RakNet::BitStream& bs,                                      \
                  FOMNetwork::Packet::TYPE& data) const;                      \
  };

#define SERIALIZER_WRITE(TYPE, FIELD)                                         \
  class FOM_API TYPE##Serializer : public BaseSerializer, public IWriter {    \
   public:                                                                    \
    static TYPE##Serializer& GetInstance() {                                  \
      static TYPE##Serializer s;                                              \
      return s;                                                               \
    }                                                                         \
    void Write(RakNet::BitStream& bs, const FOMDataUnion& d) const override { \
      WriteData(bs, d.FIELD);                                                 \
    }                                                                         \
    void WriteData(RakNet::BitStream& bs,                                     \
                   const FOMNetwork::Packet::TYPE& v) const;                  \
  };

#define SERIALIZER_READ(TYPE)                                              \
  class FOM_API TYPE##Serializer : public BaseSerializer, public IReader { \
   public:                                                                 \
    static TYPE##Serializer& GetInstance() {                               \
      static TYPE##Serializer s;                                           \
      return s;                                                            \
    }                                                                      \
    bool Read(RakNet::BitStream& bs, uint8_t* dataBuffer) const override { \
      FOMNetwork::Packet::TYPE* data =                                     \
          reinterpret_cast<FOMNetwork::Packet::TYPE*>(dataBuffer);         \
      return ReadData(bs, *data);                                          \
    }                                                                      \
    bool ReadData(RakNet::BitStream& bs,                                   \
                  FOMNetwork::Packet::TYPE& data) const;                   \
  };

/**
 * Declare all of the serializers. Keep in mind that they must be:
 * <PacketTypeName>Serializer
 */
SERIALIZER_READ(LoginRequest)
SERIALIZER_WRITE(LoginRequestReturn, loginRequestReturn)
SERIALIZER_READ(Login)
SERIALIZER_WRITE(LoginReturn, loginReturn)
SERIALIZER_READ(CheckName)
SERIALIZER_WRITE(CheckNameReturn, checkNameReturn)
SERIALIZER_READ(CreateCharacter)
SERIALIZER_BOTH(RegisterWorld, registerWorld)
SERIALIZER_READ(WorldOverview, worldOverview)
SERIALIZER_WRITE(WorldOverviewReturn, worldOverviewReturn)
SERIALIZER_READ(WorldLogin)
SERIALIZER_WRITE(WorldLoginReturn, worldLoginReturn)
SERIALIZER_BOTH(PlayerEnteringWorld, playerEnteringWorld)
SERIALIZER_BOTH(PlayerEnteringWorldReturn, playerEnteringWorldReturn)

}  // namespace FOMNetwork
