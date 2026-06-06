cmake_minimum_required(VERSION 3.26)

project(FOMServer)

# Load custom CMake modules.
list(APPEND CMAKE_MODULE_PATH "${CMAKE_SOURCE_DIR}/cmake")
include(props)

include(CTest)
enable_testing()

# Disable any optimizations incompatible with RakNet.
if(CMAKE_CXX_COMPILER_ID MATCHES "GNU|Clang")
  add_compile_options(-fno-tree-vectorize)
endif()

add_subdirectory(extern/raknet)
add_subdirectory(fom-network)
