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
  "src/packet-serializers/LoginRequestSerializer.cpp"
  "src/packet-serializers/LoginRequestReturnSerializer.cpp"
  "src/packet-serializers/LoginSerializer.cpp"
  "src/packet-serializers/LoginReturnSerializer.cpp"
  "src/packet-serializers/CheckNameSerializer.cpp"
  "src/packet-serializers/CheckNameReturnSerializer.cpp"
  "src/packet-serializers/CreateCharacterSerializer.cpp"
  "src/packet-serializers/RegisterWorldSerializer.cpp"
)
set(FOMNETWORK_INC
  "include/fom-network/ClientAPI.h"
  "include/fom-network/Common.h"
  "include/fom-network/FOMDataSerializer.h"
  "include/fom-network/FOMNetworkExport.h"
  "include/fom-network/FOMPacket.h"
  "include/fom-network/NetworkAPI.h"
  "include/fom-network/PacketAPI.h"
  "include/fom-network/PacketIdentifier.h"
  "include/fom-network/PacketSerializers.h"
  "include/fom-network/ServerAPI.h"

  "src/model-serializers/ModelSerializer.h"
  "src/model-serializers/NetworkAddressSerializer.h"
  "src/model-serializers/AvatarSerializer.h"
  "src/model-serializers/OverviewWorldSerializer.h"

  "include/fom-network/models/NetworkAddress.h"
  "include/fom-network/models/Avatar.h"
  "include/fom-network/models/OverviewWorld.h"

  "include/fom-network/packets/ReadPacketError.h"
  "include/fom-network/packets/LoginRequest.h"
  "include/fom-network/packets/LoginRequestReturn.h"
  "include/fom-network/packets/Login.h"
  "include/fom-network/packets/LoginReturn.h"
  "include/fom-network/packets/CheckName.h"
  "include/fom-network/packets/CheckNameReturn.h"
  "include/fom-network/packets/CreateCharacter.h"
  "include/fom-network/packets/RegisterWorld.h"
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
