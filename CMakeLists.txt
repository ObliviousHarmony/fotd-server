cmake_minimum_required(VERSION 3.26)

project(FOMServer)

# Load custom CMake modules.
list(APPEND CMAKE_MODULE_PATH "${CMAKE_SOURCE_DIR}/cmake")
include(props)

include(CTest)
enable_testing()

add_subdirectory(extern/raknet)
add_subdirectory(fom-network)
