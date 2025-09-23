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
  "src/serializers/LoginRequestSerializer.cpp"
  "src/serializers/LoginRequestReturnSerializer.cpp"
  "src/serializers/LoginSerializer.cpp"
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
  "include/fom-network/packets/ReadPacketError.h"
  "include/fom-network/packets/LoginRequest.h"
  "include/fom-network/packets/LoginRequestReturn.h"
  "include/fom-network/packets/Login.h"
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
