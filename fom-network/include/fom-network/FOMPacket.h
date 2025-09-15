#pragma once

#include <type_traits>
#include <fom-network/PacketIdentifier.h>

/**
 * All network packets MUST be C# blittable types. This ensures that
 * they are able to be passed between C++ and C# without needing
 * to marshall the data into managed structures. The benefit of
 * is that it doesn't require using managed memory and uses the
 * same memory without copying it.
 */
#if defined(__GNUC__) && (__GNUC__ < 5)
	// Old GCC: use deprecated traits
#define ASSERT_BLITTABLE(T) \
		static_assert(std::has_trivial_copy_constructor<T>::value, #T " must have trivial copy ctor"); \
		static_assert(std::has_trivial_copy_assign<T>::value, #T " must have trivial copy assign"); \
		static_assert(std::is_standard_layout<T>::value, #T " must have standard layout");
#else
	// Modern compilers: use the standard trait
#define ASSERT_BLITTABLE(T) \
		static_assert(std::is_trivially_copyable<T>::value, #T " must be trivially copyable"); \
		static_assert(std::is_standard_layout<T>::value, #T " must have standard layout");
#endif

/**
 * Error codes for sending/receiving packets.
 */
enum FOMPacketErrorCode : uint8_t {
	ERROR_MISSING_PACKET_ID,
	ERROR_UNHANDLED_PACKET_ID,
	ERROR_READ
};

/**
 * Make sure that we pack the structs the same way that C# does.
 */
#pragma pack(push, 1)

/**
 * The network address for a system.
 */
struct NetworkAddress {
	uint32_t binaryAddress;
	uint16_t port;
};
ASSERT_BLITTABLE(NetworkAddress);

/**
 * An error took place when processing/sending a packet.
 */
struct FOMPacketError {
	PacketIdentifier offendingID;
	FOMPacketErrorCode errorCode;
};
ASSERT_BLITTABLE(FOMPacketError);

// ID_LOGIN_REQUEST
struct LoginRequestPacket {
	char username[19];
	uint16_t clientVersion;
};
ASSERT_BLITTABLE(LoginRequestPacket);

/**
 * A union representing all of FoM's packet data types.
 */
struct FOMData {
	union {
		FOMPacketError error;
		LoginRequestPacket loginRequest;
	};
};

/**
 * A FoM network packet to be passed across the interop boundary.
 */
struct FOMPacket {
	PacketIdentifier ID;
	NetworkAddress sender;
	FOMData data;
};

#pragma pack(pop)
