cmake_minimum_required(VERSION 3.28)

project(FOMInterface)

add_compile_definitions(
	_RAKNET_LIB
	_CRT_NONSTDC_NO_DEPRECATE
	_CRT_SECURE_NO_DEPRECATE
)

add_library(FOMInterface SHARED
	src/PacketServer.cpp
)

target_include_directories(FOMInterface
	PUBLIC
	${PROJECT_SOURCE_DIR}/include
)

target_link_libraries(FOMInterface
	PRIVATE
	RakNet
)

# Windows-specific
if(WIN32)
	set(CMAKE_MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>")
	target_link_libraries(FOMInterface
		PRIVATE
		wsock32 ws2_32
	)
endif()

install(TARGETS FOMInterface
	EXPORT FOMInterfaceTargets
	LIBRARY DESTINATION lib
	RUNTIME DESTINATION lib
	INCLUDES DESTINATION include
)
install(DIRECTORY include/ DESTINATION include)
