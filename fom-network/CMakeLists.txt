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
  "src/types/item/ItemListSerializer.cpp"
  "src/types/WorldUpdateSerializer.cpp"

  # Packets
  "src/packets/RegisterWorldSerializer.cpp"
  "src/packets/LoginTokenCheckSerializer.cpp"
  "src/packets/LoginRequestSerializer.cpp"
  "src/packets/LoginRequestReturnSerializer.cpp"
  "src/packets/LoginSerializer.cpp"
  "src/packets/CheckNameSerializer.cpp"
  "src/packets/CheckNameReturnSerializer.cpp"
  "src/packets/CreateCharacterSerializer.cpp"
  "src/packets/LoginReturnSerializer.cpp"
  "src/packets/WorldLoginSerializer.cpp"
  "src/packets/WorldLoginReturnSerializer.cpp"
  "src/packets/PlayerMigrateWorldSerializer.cpp"
  "src/packets/PlayerWorldReadySerializer.cpp"
  "src/packets/PlayerLeavingWorldSerializer.cpp"
  "src/packets/RegisterClientSerializer.cpp"
  "src/packets/RegisterClientReturnSerializer.cpp"
  "src/packets/UpdateSerializer.cpp"
  "src/packets/WorldUpdateSerializer.cpp"
  "src/packets/ChatSerializer.cpp"
  "src/packets/MoveItemsSerializer.cpp"
  "src/packets/WorldObjectsSerializer.cpp"
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
  "src/types/TypeSerializer.h"
  "include/fom-network/types/NetworkAddress.h"
  "src/types/NetworkAddressSerializer.h"
  "include/fom-network/types/Avatar.h"
  "src/types/AvatarSerializer.h"
  "include/fom-network/types/Apartment.h"
  "src/types/ApartmentSerializer.h"
  "include/fom-network/types/item/ItemBase.h"
  "src/types/item/ItemBaseSerializer.h"
  "include/fom-network/types/item/Item.h"
  "src/types/item/ItemSerializer.h"
  "include/fom-network/types/item/ItemList.h"
  "src/types/item/ItemListSerializer.h"
  "include/fom-network/types/Position.h"
  "src/types/PositionSerializer.h"
  "include/fom-network/types/PositionRotation.h"
  "src/types/PositionRotationSerializer.h"
  "include/fom-network/types/player/PlayerAttributes.h"
  "src/types/player/PlayerAttributesSerializer.h"
  "include/fom-network/types/player/PlayerSkill.h"
  "include/fom-network/types/player/PlayerSkills.h"
  "src/types/player/PlayerSkillsSerializer.h"
  "include/fom-network/types/faction/FactionEmblemLayer.h"
  "src/types/faction/FactionEmblemLayerSerializer.h"
  "include/fom-network/types/faction/FactionEmblem.h"
  "src/types/faction/FactionEmblemSerializer.h"
  "include/fom-network/types/faction/FactionPerk.h"
  "include/fom-network/types/faction/FactionPerks.h"
  "src/types/faction/FactionPerksSerializer.h"
  "include/fom-network/types/WorldUpdate.h"
  "src/types/WorldUpdateSerializer.h"
  "include/fom-network/types/world/WorldObject.h"
  "src/types/world/WorldObjectSerializer.h"
  "include/fom-network/types/world/WorldService.h"
  "src/types/world/WorldServiceSerializer.h"
  "include/fom-network/types/world/WorldServiceControl.h"
  "src/types/world/WorldServiceControlSerializer.h"

  # RakNet Packets
  "include/fom-network/packets/raknet/AlreadyConnected.h"
  "include/fom-network/packets/raknet/ConnectionAttemptFailed.h"
  "include/fom-network/packets/raknet/ConnectionBanned.h"
  "include/fom-network/packets/raknet/ConnectionLost.h"
  "include/fom-network/packets/raknet/ConnectionRequestAccepted.h"
  "include/fom-network/packets/raknet/DisconnectionNotification.h"
  "include/fom-network/packets/raknet/InvalidPassword.h"
  "include/fom-network/packets/raknet/ModifiedPacket.h"
  "include/fom-network/packets/raknet/NewIncomingConnection.h"
  "include/fom-network/packets/raknet/NoFreeIncomingConnections.h"
  "include/fom-network/packets/raknet/RsaPublicKeyMismatch.h"

  # Game Packets
  "src/packets/PacketSerializers.h"
  "include/fom-network/packets/RegisterWorld.h"
  "include/fom-network/packets/LoginTokenCheck.h"
  "include/fom-network/packets/LoginRequest.h"
  "include/fom-network/packets/LoginRequestReturn.h"
  "include/fom-network/packets/Login.h"
  "include/fom-network/packets/CheckName.h"
  "include/fom-network/packets/CheckNameReturn.h"
  "include/fom-network/packets/CreateCharacter.h"
  "include/fom-network/packets/LoginReturn.h"
  "include/fom-network/packets/WorldLogin.h"
  "include/fom-network/packets/WorldLoginReturn.h"
  "include/fom-network/packets/PlayerMigrateWorld.h"
  "include/fom-network/packets/PlayerWorldReady.h"
  "include/fom-network/packets/PlayerLeavingWorld.h"
  "include/fom-network/packets/RegisterClient.h"
  "include/fom-network/packets/RegisterClientReturn.h"
  "include/fom-network/packets/Update.h"
  "include/fom-network/packets/WorldUpdate.h"
  "include/fom-network/packets/Chat.h"
  "include/fom-network/packets/MoveItems.h"
  "include/fom-network/packets/WorldObjects.h"
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
