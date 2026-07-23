project(FOMNetwork)

# Use C++11 so that we can take advantage of some of its features.
set(CMAKE_CXX_STANDARD 11)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

set(FOMNETWORK_SRC
  # Core API
  "src/NetworkApi.cpp"
  "src/ServerApi.cpp"
  "src/ClientApi.cpp"
  "src/PacketApi.cpp"
  "src/FOMDataSerializer.cpp"

  # Types
  "src/structs/item/ItemListInteropSerializer.cpp"
  "src/structs/WorldUpdateInteropSerializer.cpp"

  # Packets
  "src/packets/RegisterWorldPacketSerializer.cpp"
  "src/packets/LoginTokenCheckPacketSerializer.cpp"
  "src/packets/LoginRequestPacketSerializer.cpp"
  "src/packets/LoginRequestReturnPacketSerializer.cpp"
  "src/packets/LoginPacketSerializer.cpp"
  "src/packets/CheckNamePacketSerializer.cpp"
  "src/packets/CheckNameReturnPacketSerializer.cpp"
  "src/packets/CreateCharacterPacketSerializer.cpp"
  "src/packets/LoginReturnPacketSerializer.cpp"
  "src/packets/WorldLoginPacketSerializer.cpp"
  "src/packets/WorldLoginReturnPacketSerializer.cpp"
  "src/packets/PlayerMigrateWorldPacketSerializer.cpp"
  "src/packets/PlayerWorldReadyPacketSerializer.cpp"
  "src/packets/PlayerLeavingWorldPacketSerializer.cpp"
  "src/packets/RegisterClientPacketSerializer.cpp"
  "src/packets/RegisterClientReturnPacketSerializer.cpp"
  "src/packets/UpdatePacketSerializer.cpp"
  "src/packets/WorldUpdatePacketSerializer.cpp"
  "src/packets/ChatPacketSerializer.cpp"
  "src/packets/MoveItemsPacketSerializer.cpp"
  "src/packets/WorldObjectsPacketSerializer.cpp"
  "src/packets/WorldServicePacketSerializer.cpp"
)
set(FOMNETWORK_INC
  # Core API
  "include/fom-network/InteropTypes.h"
  "include/fom-network/Interop.h"
  "include/fom-network/NetworkApi.h"
  "include/fom-network/ServerApi.h"
  "include/fom-network/ClientApi.h"
  "include/fom-network/PacketApi.h"
  "src/RakNetIncludes.h"
  "src/FOMDataSerializer.h"
  "src/BaseSerializer.h"

  # Enums
  "include/fom-network/enums/PacketIdentifier.h"
  "include/fom-network/enums/SerializationStatus.h"
  "include/fom-network/enums/AccountType.h"
  "include/fom-network/enums/WorldId.h"
  "include/fom-network/enums/Avatar.h"
  "include/fom-network/enums/item/ItemType.h"
  "include/fom-network/enums/item/ItemSlotType.h"
  "include/fom-network/enums/item/ItemContainerType.h"
  "include/fom-network/enums/item/ItemSecurity.h"
  "include/fom-network/enums/item/ItemQuality.h"
  "include/fom-network/enums/ApartmentType.h"
  "include/fom-network/enums/SkillType.h"
  "include/fom-network/enums/AttributeType.h"
  "include/fom-network/enums/ChatChannel.h"
  "include/fom-network/enums/WorldObjectType.h"
  "include/fom-network/enums/WorldServiceType.h"

  # Constants
  "include/fom-network/constants/BufferSizes.h"
  "include/fom-network/constants/FactionConstants.h"
  "include/fom-network/constants/PlayerConstants.h"

  # Types
  "src/structs/InteropTypeSerializer.h"
  "include/fom-network/NetworkAddress.h"
  "src/NetworkAddressSerializer.h"
  "include/fom-network/structs/AvatarInterop.h"
  "src/structs/AvatarInteropSerializer.h"
  "include/fom-network/structs/ApartmentInterop.h"
  "src/structs/ApartmentInteropSerializer.h"
  "include/fom-network/structs/item/ItemBaseInterop.h"
  "src/structs/item/ItemBaseInteropSerializer.h"
  "include/fom-network/structs/item/ItemInterop.h"
  "src/structs/item/ItemInteropSerializer.h"
  "include/fom-network/structs/item/ItemListInterop.h"
  "src/structs/item/ItemListInteropSerializer.h"
  "include/fom-network/structs/PositionInterop.h"
  "src/structs/PositionInteropSerializer.h"
  "include/fom-network/structs/PositionRotationInterop.h"
  "src/structs/PositionRotationInteropSerializer.h"
  "include/fom-network/structs/player/PlayerAttributesInterop.h"
  "src/structs/player/PlayerAttributesInteropSerializer.h"
  "include/fom-network/structs/player/PlayerSkillInterop.h"
  "include/fom-network/structs/player/PlayerSkillsInterop.h"
  "src/structs/player/PlayerSkillsInteropSerializer.h"
  "include/fom-network/structs/faction/FactionEmblemLayerInterop.h"
  "src/structs/faction/FactionEmblemLayerInteropSerializer.h"
  "include/fom-network/structs/faction/FactionEmblemInterop.h"
  "src/structs/faction/FactionEmblemInteropSerializer.h"
  "include/fom-network/structs/faction/FactionPerkInterop.h"
  "include/fom-network/structs/faction/FactionPerksInterop.h"
  "src/structs/faction/FactionPerksInteropSerializer.h"
  "include/fom-network/structs/WorldUpdateInterop.h"
  "src/structs/WorldUpdateInteropSerializer.h"
  "include/fom-network/structs/world/WorldObjectInterop.h"
  "src/structs/world/WorldObjectInteropSerializer.h"
  "include/fom-network/structs/world/WorldServiceInterop.h"
  "src/structs/world/WorldServiceInteropSerializer.h"
  "include/fom-network/structs/world/WorldServiceControlInterop.h"
  "src/structs/world/WorldServiceControlInteropSerializer.h"

  # RakNet Packets
  "include/fom-network/packets/raknet/AlreadyConnectedPacket.h"
  "include/fom-network/packets/raknet/ConnectionAttemptFailedPacket.h"
  "include/fom-network/packets/raknet/ConnectionBannedPacket.h"
  "include/fom-network/packets/raknet/ConnectionLostPacket.h"
  "include/fom-network/packets/raknet/ConnectionRequestAcceptedPacket.h"
  "include/fom-network/packets/raknet/DisconnectionNotificationPacket.h"
  "include/fom-network/packets/raknet/InvalidPasswordPacket.h"
  "include/fom-network/packets/raknet/ModifiedPacketPacket.h"
  "include/fom-network/packets/raknet/NewIncomingConnectionPacket.h"
  "include/fom-network/packets/raknet/NoFreeIncomingConnectionsPacket.h"
  "include/fom-network/packets/raknet/RsaPublicKeyMismatchPacket.h"

  # Game Packets
  "src/packets/PacketSerializers.h"
  "include/fom-network/packets/RegisterWorldPacket.h"
  "include/fom-network/packets/LoginTokenCheckPacket.h"
  "include/fom-network/packets/LoginRequestPacket.h"
  "include/fom-network/packets/LoginRequestReturnPacket.h"
  "include/fom-network/packets/LoginPacket.h"
  "include/fom-network/packets/CheckNamePacket.h"
  "include/fom-network/packets/CheckNameReturnPacket.h"
  "include/fom-network/packets/CreateCharacterPacket.h"
  "include/fom-network/packets/LoginReturnPacket.h"
  "include/fom-network/packets/WorldLoginPacket.h"
  "include/fom-network/packets/WorldLoginReturnPacket.h"
  "include/fom-network/packets/PlayerMigrateWorldPacket.h"
  "include/fom-network/packets/PlayerWorldReadyPacket.h"
  "include/fom-network/packets/PlayerLeavingWorldPacket.h"
  "include/fom-network/packets/RegisterClientPacket.h"
  "include/fom-network/packets/RegisterClientReturnPacket.h"
  "include/fom-network/packets/UpdatePacket.h"
  "include/fom-network/packets/WorldUpdatePacket.h"
  "include/fom-network/packets/ChatPacket.h"
  "include/fom-network/packets/MoveItemsPacket.h"
  "include/fom-network/packets/WorldObjectsPacket.h"
  "include/fom-network/packets/WorldServicePacket.h"
)
set(FOMNETWORK_TEST
    "tests/FOMDataSerializer_test.cpp"
)

# Object library - compiled once, used by both DLL and tests
add_library(FOMNetwork_obj OBJECT ${FOMNETWORK_SRC})
target_include_directories(FOMNetwork_obj
    PUBLIC
    "${PROJECT_SOURCE_DIR}/include"
)
target_link_libraries(FOMNetwork_obj
    PUBLIC
    RakNet
)
if(UNIX AND NOT WIN32)
    set_target_properties(FOMNetwork_obj PROPERTIES POSITION_INDEPENDENT_CODE ON)
endif()

# Shared library for consumers
add_library(FOMNetwork SHARED $<TARGET_OBJECTS:FOMNetwork_obj>)
target_include_directories(FOMNetwork
    PUBLIC
    "${PROJECT_SOURCE_DIR}/include"
)
target_link_libraries(FOMNetwork
    PRIVATE
    RakNet
)

if(WIN32)
    set(ADDITIONAL_INCLUDES
        "$(SolutionDir)extern/raknet/include"
        "$(SolutionDir)out/build/$(Configuration)/_deps/googletest-src/googletest/include"
        "include"
    )
    generate_vcx_props(
        FOMNetwork
        SOURCES ${FOMNETWORK_SRC}
        HEADERS ${FOMNETWORK_INC}
        TESTS ${FOMNETWORK_TEST}
        INCLUDES ${ADDITIONAL_INCLUDES}
    )
endif()

# Windows-specific
if(WIN32)
    set(CMAKE_MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>DLL")
    target_link_libraries(FOMNetwork
        PRIVATE
        wsock32 ws2_32
    )
endif()

# Linux-specific
if(UNIX AND NOT WIN32)
    target_compile_options(FOMNetwork PRIVATE -fvisibility=hidden -fvisibility-inlines-hidden)
endif()

add_subdirectory(tests)
