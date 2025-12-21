project(FOMNetwork)

# Use C++11 so that we can take advantage of some of its features.
set(CMAKE_CXX_STANDARD 11)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

set(FOMNETWORK_SRC
    "src/ClientAPI.cpp"
    "src/ServerAPI.cpp"
    "src/PacketAPI.cpp"
    "src/NetworkAPI.cpp"
    "src/FOMDataSerializer.cpp"
    "src/packet-serializers/data/RegisterWorldSerializer.cpp"
 )
set(FOMNETWORK_INC
    "include/fom-network/FOMNetworkExport.h"
    "include/fom-network/Common.h"
    "include/fom-network/FOMNetworkTypes.h"
    "include/fom-network/ClientAPI.h"
    "include/fom-network/ServerAPI.h"
    "include/fom-network/PacketAPI.h"
    "include/fom-network/NetworkAPI.h"
    "include/fom-network/FOMDataSerializer.h"

    "include/fom-network/enums/EquipmentSlot.h"
    "include/fom-network/enums/Faction.h"
    "include/fom-network/enums/FactionRelation.h"
    "include/fom-network/enums/ItemType.h"
    "include/fom-network/enums/MedicalTreatment.h"
    "include/fom-network/enums/PlayerAttachment.h"
    "include/fom-network/enums/PlayerAttribute.h"
    "include/fom-network/enums/WorldID.h"
    "include/fom-network/enums/WorldUpdateType.h"
  
    "include/fom-network/packets/PacketIdentifier.h"
    "include/fom-network/packets/PacketTypes.h"
    "include/fom-network/packets/PacketSerializers.h"
    "src/packet-serializers/models/ModelSerializer.h"
    "include/fom-network/packets/NetworkAddress.h"
    "src/packet-serializers/NetworkAddressSerializer.h"

    "include/fom-network/packets/data/raknet/AlreadyConnected.h"
    "include/fom-network/packets/data/raknet/ConnectionAttemptFailed.h"
    "include/fom-network/packets/data/raknet/ConnectionBanned.h"
    "include/fom-network/packets/data/raknet/ConnectionLost.h"
    "include/fom-network/packets/data/raknet/ConnectionRequestAccepted.h"
    "include/fom-network/packets/data/raknet/DisconnectionNotification.h"
    "include/fom-network/packets/data/raknet/InvalidPassword.h"
    "include/fom-network/packets/data/raknet/ModifiedPacket.h"
    "include/fom-network/packets/data/raknet/NewIncomingConnection.h"
    "include/fom-network/packets/data/raknet/NoFreeIncomingConnections.h"
    "include/fom-network/packets/data/raknet/RSAPublicKeyMismatch.h"

    "include/fom-network/packets/data/ReadPacketError.h"
    "include/fom-network/packets/data/RegisterWorld.h"
)
set(FOMNETWORK_TEST
    "tests/FOMDataSerializer_test.cpp"
)

add_library(FOMNetwork SHARED ${FOMNETWORK_SRC})

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
