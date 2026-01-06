#pragma once

#include <fom-network/Interop.h>

#ifdef __cplusplus
extern "C" {
#endif

/**
 * Starts an interface for sending and receiving packets as a server.
 *
 * @param port The port to listen for incoming connections on.
 * @return A pointer to the initialized interface, or null on failure.
 */
FOM_API FOMNetworkPeer* FOMNetwork_Server_Startup(uint16_t port);

/**
 * Shuts down the server interface.
 *
 * @param peer The interface instance to shut down.
 */
FOM_API void FOMNetwork_Server_Shutdown(FOMNetworkPeer* server);

#ifdef __cplusplus
}
#endif
