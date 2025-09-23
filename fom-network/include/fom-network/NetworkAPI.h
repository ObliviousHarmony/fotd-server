#pragma once

#include <fom-network/Common.h>
#include <fom-network/FOMNetworkExport.h>
#include <fom-network/PacketIdentifier.h>
#include <raknet/RakPeer.h>

#pragma pack(push, 1)

/**
 * Contains information about packet structures to be used to
 * validate that unmanaged and managed code agree on their
 * size. This does not guarantee they are compatible but
 * it's a confidence check to pair with consumer
 * validation.
 *
 * @note This structure MUST only contain C# blittable types.
 */
struct PacketStructure {
  PacketIdentifier id;
  int32_t size;
};

#pragma pack(pop)

#ifdef __cplusplus
extern "C" {
#endif

/**
 * Validates the consumer's packet structures to make sure
 * they are compatible with the network API.
 *
 * @return int32_t The status code.
 * @retval 0 Success.
 * @retval -1 Mismatched struct count.
 * @retval -2 Library missing struct.
 * @retval -3 Mismatched struct size.
 */
FOM_API int32_t FOMNetwork_ValidatePacketStructs(
    const PacketStructure* structures, int32_t count);

#ifdef __cplusplus
}
#endif
