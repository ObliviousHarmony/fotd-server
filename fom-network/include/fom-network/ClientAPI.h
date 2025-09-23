#pragma once

#include <fom-network/Common.h>
#include <fom-network/FOMNetworkExport.h>
#include <raknet/RakPeerInterface.h>

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
FOM_API RakPeerInterface* FOMNetwork_Client_Connect(const char* hostAddress,
                                                    uint16_t port);

/**
 * Disconnects from the server and shuts down the client interface.
 *
 * @param client The interface instance to disconnect and shut down.
 */
FOM_API void FOMNetwork_Client_Disconnect(RakPeerInterface* client);

#ifdef __cplusplus
}
#endif
