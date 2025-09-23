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
  void EncodeString(RakNet::BitStream& bs, const char (&input)[N]) const {
    strCompressor->EncodeString(input, N, &bs);
  }

  template <size_t N>
  bool DecodeString(RakNet::BitStream& bs, char (&output)[N]) const {
    return strCompressor->DecodeString(output, N, &bs);
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
