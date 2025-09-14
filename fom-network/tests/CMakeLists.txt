# GoogleTest requires at least C++11
set(CMAKE_CXX_STANDARD 11)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

include(FetchContent)
FetchContent_Declare(
	googletest
	URL https://github.com/google/googletest/archive/6a7ed316a5cdc07b6d26362c90770787513822d4.zip
)
# For Windows: Prevent overriding the parent project's compiler/linker settings
set(gtest_force_shared_crt ON CACHE BOOL "" FORCE)
FetchContent_MakeAvailable(googletest)

# Create the test executable FIRST
add_executable(FOMNetworkTests
)

target_include_directories(FOMNetworkTests PRIVATE
	${PROJECT_SOURCE_DIR}/include
	${PROJECT_SOURCE_DIR}/src
)

target_link_libraries(FOMNetworkTests
	PRIVATE
	FOMNetwork
	RakNet
	gtest_main
)

# Now add the POST_BUILD copy for Windows
if (MSVC)
	set(CMAKE_MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>DLL")

	# Disable Microsoft Code Analysis for GoogleTest targets
	set_target_properties(gtest PROPERTIES VS_GLOBAL_EnableMicrosoftCodeAnalysis false)
	set_target_properties(gtest_main PROPERTIES VS_GLOBAL_EnableMicrosoftCodeAnalysis false)

	# Copy FOMNetwork.dll to the test executable directory
	add_custom_command(TARGET FOMNetworkTests POST_BUILD
		COMMAND ${CMAKE_COMMAND} -E copy_if_different
			$<TARGET_FILE:FOMNetwork>
			$<TARGET_FILE_DIR:FOMNetworkTests>
	)
endif()

# Discover the GoogleTest tests
include(GoogleTest)
gtest_discover_tests(FOMNetworkTests
	EXTRA_ARGS --gtest_color=yes
)
