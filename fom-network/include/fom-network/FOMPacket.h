#pragma once

#include <type_traits>
#include <fom-network/PacketIdentifiers.h>

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
 * Make sure that we pack the structs the same way that C# does.
 */
#pragma pack(push, 1)

/**
 * An example packet structure.
 */
struct ExamplePacket {
	uint8_t exampleField1;
};
ASSERT_BLITTABLE(ExamplePacket);

/**
 * A discriminated union representing all of FoM's network packets.
 */
struct FOMPacket {
	PacketIdentifier ID;

	union
	{
		ExamplePacket example;
	} data;
};

#define INVALID_PACKET FOMPacket{ (PacketIdentifier)INVALID_PACKET_ID }

#pragma pack(pop)
