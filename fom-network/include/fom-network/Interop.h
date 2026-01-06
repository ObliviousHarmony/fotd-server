#pragma once

#include <fom-network/InteropTypes.h>

#include <type_traits>

#if defined(_WIN32) || defined(_WIN64)
#define FOM_API __declspec(dllexport)
#else
#define FOM_API __attribute__((visibility("default")))
#endif

/**
 * The internal peer used in all network operations.
 */
typedef void FOMNetworkPeer;

/**
 * An internal raw network packet.
 */
typedef void FOMNetworkRawPacket;

/**
 * All network packets MUST be C# blittable types. This ensures that
 * they are able to be passed between C++ and C# without needing
 * to marshall the data into managed structures. The benefit of
 * is that it doesn't require using managed memory and uses the
 * same memory without copying it.
 */
#if defined(__GNUC__) && (__GNUC__ < 5)
// Old GCC: use deprecated traits
#define ASSERT_BLITTABLE(T)                                  \
  static_assert(std::has_trivial_copy_constructor<T>::value, \
                #T " must have trivial copy ctor");          \
  static_assert(std::has_trivial_copy_assign<T>::value,      \
                #T " must have trivial copy assign");        \
  static_assert(std::is_standard_layout<T>::value,           \
                #T " must have standard layout");
#else
// Modern compilers: use the standard trait
#define ASSERT_BLITTABLE(T)                           \
  static_assert(std::is_trivially_copyable<T>::value, \
                #T " must be trivially copyable");    \
  static_assert(std::is_standard_layout<T>::value,    \
                #T " must have standard layout");
#endif
