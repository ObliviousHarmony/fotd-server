cmake_minimum_required(VERSION 3.28)

project(FOMServer LANGUAGES CSharp)

add_executable(FOMServer
    Program.cs
)

set_property(TARGET FOMServer PROPERTY DOTNET_TARGET_FRAMEWORK "net6.0")

# Windows-specific
if(WIN32)
    set_property(TARGET FOMServer PROPERTY DOTNET_RUNTIME_IDENTIFIER "win-x64")
endif()

# Linux-specific
if(UNIX)
	set_property(TARGET FOMServer PROPERTY DOTNET_RUNTIME_IDENTIFIER "linux-x64")
endif()

# Make sure we're including the interface DLL.
add_dependencies(FOMServer FOMInterface) 

install(TARGETS FOMServer
    EXPORT FOMServerTargets
    RUNTIME DESTINATION bin
)
