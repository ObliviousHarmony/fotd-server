#pragma once

#include <raknet/RakPeer.h>
#include <fom-network/Common.h>
#include <fom-network/PacketIdentifiers.h>
#include <fom-network/FOMNetworkExport.h>

/**
 * Contains information about packet structures to be used to
 * validate that unmanaged and managed code agree on their
 * size. This does not guarantee they are compatible but
 * it's a confidence check to pair with consumer
 * validation.
 */
struct PacketStructure {
	PacketIdentifier id;
	uint32_t size;
};

#ifdef __cplusplus
extern "C" {
#endif

	/**
	 * Validates the consumer's packet structures to make sure
	 * they are compatible with the network API.
	 * 
	 * @return int8_t The status code.
	 * @retval 0 Success.
	 * @retval -1 Mismatched struct count.
	 * @retval -2 Library missing struct.
	 * @retval -3 Mismatched struct size.
	 */ 
	FOM_API int8_t FOMNetwork_ValidatePacketStructs(const PacketStructure* structures, uint32_t count);

#ifdef __cplusplus
}
#endif
