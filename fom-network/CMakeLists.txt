project(FOMNetwork)

# Use C++11 so that we can take advantage of some of its features.
set(CMAKE_CXX_STANDARD 11)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

set(FOMNETWORK_SRC
  # Core API
  "src/NetworkAPI.cpp"
  "src/ServerAPI.cpp"
  "src/ClientAPI.cpp"
  "src/PacketAPI.cpp"
  "src/FOMDataSerializer.cpp"

  # Types
  "src/types/ItemListSerializer.cpp"

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
)
set(FOMNETWORK_INC
  # Core API
  "include/fom-network/InteropTypes.h"
  "include/fom-network/Interop.h"
  "include/fom-network/NetworkAPI.h"
  "include/fom-network/ServerAPI.h"
  "include/fom-network/ClientAPI.h"
  "include/fom-network/PacketAPI.h"
  "src/RakNetIncludes.h"
  "src/FOMDataSerializer.h"
  "src/BaseSerializer.h"

  # Enums
  "include/fom-network/enums/PacketIdentifier.h"
  "include/fom-network/enums/SerializationStatus.h"
  "include/fom-network/enums/AccountType.h"
  "include/fom-network/enums/WorldID.h"
  "include/fom-network/enums/Player.h"
  "include/fom-network/enums/ItemType.h"
  "include/fom-network/enums/ItemSecurity.h"
  "include/fom-network/enums/ItemQuality.h"
  "include/fom-network/enums/ApartmentType.h"
  "include/fom-network/enums/PlayerSkill.h"
  "include/fom-network/enums/PlayerAttribute.h"

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
  "include/fom-network/types/ItemBase.h"
  "src/types/ItemBaseSerializer.h"
  "include/fom-network/types/Item.h"
  "src/types/ItemSerializer.h"
  "include/fom-network/types/ItemList.h"
  "src/types/ItemListSerializer.h"
  "include/fom-network/types/Position.h"
  "src/types/PositionSerializer.h"
  "include/fom-network/types/PlayerAttributes.h"
  "src/types/PlayerAttributesSerializer.h"
  "include/fom-network/types/PlayerSkill.h"
  "include/fom-network/types/PlayerSkills.h"
  "src/types/PlayerSkillsSerializer.h"
  "include/fom-network/types/FactionEmblemLayer.h"
  "src/types/FactionEmblemLayerSerializer.h"
  "include/fom-network/types/FactionEmblem.h"
  "src/types/FactionEmblemSerializer.h"
  "include/fom-network/types/FactionPerk.h"
  "include/fom-network/types/FactionPerks.h"
  "src/types/FactionPerksSerializer.h"

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
  "include/fom-network/packets/raknet/RSAPublicKeyMismatch.h"

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
