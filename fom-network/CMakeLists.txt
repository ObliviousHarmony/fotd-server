cmake_minimum_required(VERSION 3.28)

project(FOMNetwork)

# Use C++11 so that we can take advantage of some of its features.
set(CMAKE_CXX_STANDARD 11)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

add_library(FOMNetwork SHARED
	src/ClientAPI.cpp
	src/ServerAPI.cpp
	src/PacketAPI.cpp
)

target_include_directories(FOMNetwork
	PUBLIC
	${PROJECT_SOURCE_DIR}/include
)

target_link_libraries(FOMNetwork
	PRIVATE
	RakNet
)

# Windows-specific
if(WIN32)
	set(CMAKE_MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>")
	target_link_libraries(FOMNetwork
		PRIVATE
		wsock32 ws2_32
	)
endif()
