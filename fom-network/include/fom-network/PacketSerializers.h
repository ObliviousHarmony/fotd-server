#pragma once

#include <raknet/BitStream.h>
#include <fom-network/FOMNetworkExport.h>
#include <fom-network/FOMPacket.h>

/**
 * Base interfaces for packet serializers.
 */
struct IWriter {
	virtual ~IWriter() = default;
	virtual bool Write(RakNet::BitStream& bs, const FOMData& data) const = 0;
};

struct IReader {
	virtual ~IReader() = default;
	virtual FOMData Read(RakNet::BitStream& bs) const = 0;
};

/**
 * Macros to reduce serializer boilerplate.
 */
#define SERIALIZER_BOTH(TYPE, FIELD)										\
class FOM_API TYPE##Serializer : public IWriter, public IReader {			\
public:																		\
	static TYPE##Serializer& Get() { static TYPE##Serializer s; return s; }	\
	bool Write(RakNet::BitStream& bs, const FOMData& d) const override { 	\
		return Write(bs, d.FIELD);											\
	}																		\
	FOMData Read(RakNet::BitStream& bs) const override {					\
		FOMData data{};														\
		data.FIELD = Read(bs);												\
		return data;														\
	}																		\
	bool Write(RakNet::BitStream& bs, const TYPE& v) const;					\
	TYPE Read(RakNet::BitStream& bs) const;									\
};

#define SERIALIZER_WRITE(TYPE, FIELD)										\
class FOM_API TYPE##Serializer : public IWriter {							\
public:																		\
	static TYPE##Serializer& Get() { static TYPE##Serializer s; return s; }	\
	bool Write(RakNet::BitStream& bs, const FOMData& d) const override { 	\
		return Write(bs, d.FIELD);											\
	}																		\
	bool Write(RakNet::BitStream& bs, const TYPE& v) const;					\
};

#define SERIALIZER_READ(TYPE, FIELD)										\
class FOM_API TYPE##Serializer : public IReader {							\
public:																		\
	static TYPE##Serializer& Get() { static TYPE##Serializer s; return s; }	\
	FOMData Read(RakNet::BitStream& bs) const override {					\
		FOMData data{};														\
		data.FIELD = Read(bs);												\
		return data;														\
	}																		\
	TYPE Read(RakNet::BitStream& bs) const;									\
};

/**
 * Declare all of the serializers. Keep in mind that they must be:
 * <PacketTypeName>Serializer
 */
