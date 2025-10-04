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
  "src/packet-serializers/data/LoginRequestSerializer.cpp"
  "src/packet-serializers/data/LoginRequestReturnSerializer.cpp"
  "src/packet-serializers/data/LoginSerializer.cpp"
  "src/packet-serializers/data/LoginReturnSerializer.cpp"
  "src/packet-serializers/data/CheckNameSerializer.cpp"
  "src/packet-serializers/data/CheckNameReturnSerializer.cpp"
  "src/packet-serializers/data/CreateCharacterSerializer.cpp"
  "src/packet-serializers/data/RegisterWorldSerializer.cpp"
  "src/packet-serializers/data/WorldOverviewSerializer.cpp"
  "src/packet-serializers/data/WorldOverviewReturnSerializer.cpp"
  "src/packet-serializers/data/WorldLoginSerializer.cpp"
  "src/packet-serializers/data/WorldLoginReturnSerializer.cpp"
)
set(FOMNETWORK_INC
  "include/fom-network/FOMNetworkExport.h"
  "include/fom-network/Common.h"
  "include/fom-network/ClientAPI.h"
  "include/fom-network/ServerAPI.h"
  "include/fom-network/PacketAPI.h"
  "include/fom-network/NetworkAPI.h"
  "include/fom-network/FOMDataSerializer.h"
 
  "include/fom-network/packets/FOMPacket.h"
  "include/fom-network/packets/PacketIdentifier.h"
  "include/fom-network/packets/PacketSerializers.h"
  "src/packet-serializers/models/ModelSerializer.h"
  "include/fom-network/packets/NetworkAddress.h"
  "src/packet-serializers/NetworkAddressSerializer.h"

  "include/fom-network/packets/models/AvatarModel.h"
  "src/packet-serializers/models/AvatarModelSerializer.h"
  "include/fom-network/packets/models/ApartmentModel.h"
  "src/packet-serializers/models/ApartmentModelSerializer.h"
  "include/fom-network/packets/models/WorldOverviewModel.h"
  "src/packet-serializers/models/WorldOverviewModelSerializer.h"

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
  "include/fom-network/packets/data/LoginRequest.h"
  "include/fom-network/packets/data/LoginRequestReturn.h"
  "include/fom-network/packets/data/Login.h"
  "include/fom-network/packets/data/LoginReturn.h"
  "include/fom-network/packets/data/CheckName.h"
  "include/fom-network/packets/data/CheckNameReturn.h"
  "include/fom-network/packets/data/CreateCharacter.h"
  "include/fom-network/packets/data/RegisterWorld.h"
  "include/fom-network/packets/data/WorldOverview.h"
  "include/fom-network/packets/data/WorldOverviewReturn.h"
  "include/fom-network/packets/data/WorldLogin.h"
  "include/fom-network/packets/data/WorldLoginReturn.h"
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
