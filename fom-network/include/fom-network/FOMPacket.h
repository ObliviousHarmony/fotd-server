#pragma once

#include <fom-network/PacketIdentifiers.h>

#pragma pack(push, 1)

/**
* @file FOMPacket.h
*
* Contains the definitions for all of the network packets used by FoM.
*
* @note This file MUST only contain C# blittable types.
*/

struct ExamplePacket {
	uint8_t exampleField1;
};

/**
* A discriminated union representing all of FoM's network packets.
*/
struct FOMPacket {
	PacketIdentifier id;

	union
	{
		ExamplePacket example;
	} data;
};

#pragma pack(pop)
