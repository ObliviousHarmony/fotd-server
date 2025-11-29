cmake_minimum_required(VERSION 3.28)

project(FOMServer)

# Load custom CMake modules.
list(APPEND CMAKE_MODULE_PATH "${CMAKE_SOURCE_DIR}/cmake")
include(props)

# Use ccache if available for faster rebuilds.
find_program(CCACHE_PROGRAM ccache)
if(CCACHE_PROGRAM)
    set(CMAKE_CXX_COMPILER_LAUNCHER ${CCACHE_PROGRAM})
    set(CMAKE_C_COMPILER_LAUNCHER ${CCACHE_PROGRAM})
endif()

# Suppress CMake developer warnings.
set(CMAKE_SUPPRESS_DEVELOPER_WARNINGS 1)

include(CTest)
enable_testing()

add_subdirectory(extern/raknet)
add_subdirectory(fom-network)
