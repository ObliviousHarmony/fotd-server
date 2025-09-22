# Face of Mankind Server Emulator

This project is a server emulator for the MMORPG "Face of Mankind". It aims to replicate
the functionality of the original game servers, allowing players to experience the game
using their own private servers.

## Getting Started

### Prerequisites

- [**Docker**](https://docs.docker.com/engine/install/): In addition to a development server, Docker is necessary to build the project on Linux.
This is because the included RakNet library requires GCC 4.8 to build, which is not available on
modern operating systems.
- [**just**](https://github.com/casey/just): A command runner that simplifies working with tools.
- [**CMake 3.28 (or newer)**](https://cmake.org/download)

### Building

#### Windows

Visual Studio 2022 provides building for both C++ and C# components. Open the `FOMServer.sln` and then switch to [the "CMake Targets" view.](https://learn.microsoft.com/en-us/cpp/build/cmake-projects-in-visual-studio?view=msvc-170)
From there, "Configure" the FOMServer Project and it will generate the necessary CMake files for Visual Studio's integration. You will
now be able to build both "Master Server" and "World Server" from the solution view. If you enable
[mixed-mode debugging](https://learn.microsoft.com/en-us/visualstudio/debugger/how-to-debug-in-mixed-mode?view=vs-2022&pivots=programming-language-cpp)
you will be able to step through both C++ and C# code using the Visual Studio debugger.

#### macOS / Linux

One of the caveats is that building the C++ components requires GCC 4.8, which is ancient and not supported on modern operating systems.
To address this, a Docker container is provided that handles building the project. `just build` will generate both the C++ and C#
components for you.

### Testing

#### Windows

Visual Studio supports running the test suite using the Test Explorer.

#### macOS / Linux

```bash
# Run all tests.
just test

# Run them individually.
just test-cpp
just test-dotnet
```

### Development Server

```bash
# Start the database server.
just db-up

# Start the master server (automatically starts the database server if not running)
just ms-up
```

Using Visual Studio you can also debug the servers directly as long as a database server is running.

