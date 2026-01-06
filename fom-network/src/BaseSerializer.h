#pragma once

#include <cmath>
#include <cstring>

#include "RakNetIncludes.h"

struct IWriter {
  virtual ~IWriter() = default;
  virtual void WriteRaw(RakNet::BitStream& bs, const uint8_t* data) const = 0;
};

struct IReader {
  virtual ~IReader() = default;
  virtual bool ReadRaw(RakNet::BitStream& bs, uint8_t* dataBuffer) const = 0;
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
  bool ReadBits(RakNet::BitStream& bs, T& input, int numberOfBitsToRead) const {
    return bs.ReadBits((uint8_t*)&input, numberOfBitsToRead);
  }

  template <uint8_t N>
  void WriteString(RakNet::BitStream& bs, const uint8_t (&input)[N]) const {
    int bitsForLength = static_cast<int>(std::floor(std::log2(N) + 1));
    uint8_t len = static_cast<uint8_t>(strnlen((const char*)input, N));
    bs.WriteBits((uint8_t*)&len, bitsForLength);
    bs.WriteBits(input, len * 8);
  }

  template <uint8_t N>
  bool ReadString(RakNet::BitStream& bs, uint8_t (&output)[N]) const {
    int bitsForLength = static_cast<int>(std::floor(std::log2(N) + 1));
    uint8_t len = 0;
    if (!bs.ReadBits((uint8_t*)&len, bitsForLength)) return false;
    if (len >= N) len = N - 1;
    if (len > 0 && !bs.ReadBits(output, len * 8)) return false;
    output[len] = '\0';
    return true;
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
