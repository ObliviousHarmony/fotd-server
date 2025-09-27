#pragma once

#include <fom-network/FOMNetworkExport.h>
#include <fom-network/FOMPacket.h>
#include <raknet/BitStream.h>
#include <raknet/StringCompressor.h>

/**
 * Base interfaces for packet serializers.
 */
struct IWriter {
  virtual ~IWriter() = default;
  virtual void Write(RakNet::BitStream& bs, const FOMDataUnion& data) const = 0;
};

struct IReader {
  virtual ~IReader() = default;
  virtual FOMDataUnion Read(RakNet::BitStream& bs) const = 0;
};

class BaseSerializer {
 protected:
  BaseSerializer() {
    StringCompressor::AddReference();
    strCompressor = StringCompressor::Instance();
  }

  ~BaseSerializer() { StringCompressor::RemoveReference(); }

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

/**
 * Macros to reduce serializer boilerplate.
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
    FOMDataUnion Read(RakNet::BitStream& bs) const override {                 \
      FOMDataUnion data{};                                                    \
      data.FIELD = ReadData(bs);                                              \
      return data;                                                            \
    }                                                                         \
    void WriteData(RakNet::BitStream& bs, const FOMPacket::TYPE& v) const;    \
    FOMPacket::TYPE ReadData(RakNet::BitStream& bs) const;                    \
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
    void WriteData(RakNet::BitStream& bs, const FOMPacket::TYPE& v) const;    \
  };

#define SERIALIZER_READ(TYPE, FIELD)                                       \
  class FOM_API TYPE##Serializer : public BaseSerializer, public IReader { \
   public:                                                                 \
    static TYPE##Serializer& GetInstance() {                               \
      static TYPE##Serializer s;                                           \
      return s;                                                            \
    }                                                                      \
    FOMDataUnion Read(RakNet::BitStream& bs) const override {              \
      FOMDataUnion data{};                                                 \
      data.FIELD = ReadData(bs);                                           \
      return data;                                                         \
    }                                                                      \
    FOMPacket::TYPE ReadData(RakNet::BitStream& bs) const;                 \
  };

/**
 * Declare all of the serializers. Keep in mind that they must be:
 * <PacketTypeName>Serializer
 */
SERIALIZER_READ(LoginRequest, loginRequest)
SERIALIZER_WRITE(LoginRequestReturn, loginRequestReturn)
SERIALIZER_READ(Login, login)
SERIALIZER_WRITE(LoginReturn, loginReturn)
