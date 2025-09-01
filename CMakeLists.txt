cmake_minimum_required(VERSION 3.28)

project(FOMServer)

# Suppress CMake developer warnings.
set(CMAKE_SUPPRESS_DEVELOPER_WARNINGS 1)

include(CTest)
enable_testing()

add_subdirectory(extern/raknet)
add_subdirectory(fom-network)
