#pragma once

#include <fom-network/Interop.h>

#ifdef __cplusplus
extern "C" {
#endif

/**
 * Connects to a server at the specified address and port.
 *
 * @param hostAddress The string IP address or domain of the server to connect
 * to.
 * @param port The port number of the server to connect to.
 * @return A pointer to the initialized interface, or null on failure.
 */
FOM_API FOMNetworkPeer* FOMNetwork_Client_Connect(const char* hostAddress,
                                                  uint16_t port);

/**
 * Disconnects from the server and shuts down the client interface.
 *
 * @param client The interface instance to disconnect and shut down.
 */
FOM_API void FOMNetwork_Client_Disconnect(FOMNetworkPeer* client);

#ifdef __cplusplus
}
#endif
