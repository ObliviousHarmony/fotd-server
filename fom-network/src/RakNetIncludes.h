#pragma once

#if defined(_MSC_VER)
#pragma warning(push, 0)
#elif defined(__GNUC__) || defined(__clang__)
#pragma GCC system_header
#endif

#include <raknet/BitStream.h>
#include <raknet/GetTime.h>
#include <raknet/MessageIdentifiers.h>
#include <raknet/RakNetTypes.h>
#include <raknet/RakNetworkFactory.h>
#include <raknet/RakPeerInterface.h>
#include <raknet/RakSleep.h>
#include <raknet/StringCompressor.h>

#if defined(_MSC_VER)
#pragma warning(pop)
#endif
