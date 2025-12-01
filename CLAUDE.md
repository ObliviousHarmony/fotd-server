# Face of Mankind Emulator

This repository contains a server emulator for the discontinued MMOFPS, "Face of Mankind", first released in 2006. It features a client/server architecture with two kinds of servers, a "master" server that holds cross-server global state and individual "world" servers for player movement and interaction within a given world.

## Projects

- [RakNet 3.611](/extern/raknet) is the game's underlying network library.
- [FOMNetwork](/fom-network) contains native packet definitions, serializers, and abstracts RakNet.
- [ServerShared](/server-shared) contains the managed packet definitions and shared functionality between the "master" and "world" servers. This includes things like packet sending/receiving, persistence, and interfaces for common themes between the two.
- [MasterServer](/master-server) is for shared global state between servers and helps transfer players between world servers. This contains things like Factions, Contracts, and other global structures.
- [WorldServer](/world-server) is for world state such as players, their attributes, their items, and their interactions with the game.

### Managed Structure

- **Core**: Contains domain entities and interfaces for application-specific functionality.
- **Application**: Contains the meat of the application; orchestrating behavior from Infrastructure and Core code.
- **Infrastructure**: Interacts with external services and libraries, such as the FOMNetwork project and databases.

### Native Structure

- `include/fom-network` is for structures and code exposed to consumers.
- `src` contains source files for included headers.

## Code Style

- `.editorconfig` for formatting rules
- `.clang-format` for C++ formatting
- Roslyn analyzers for C# formatting

### Special Rules

- Capitalize acronyms, not just the first letter (e.g., `ID` not `Id`, `HTTP` not `Http`).

## Common Commands

- **C++ Format**: `just format-cpp`
- **C# Format**: `just format-dotnet`

### Windows

- **C++ Debug Build**: `cmake --preset Debug-Windows && cmake --build --preset Debug-Windows`
- **C++ Debug Test**: `ctest --test-dir out/build/Debug`
- **C# Build**: `dotnet build ManagedOnly.slnf`
- **C# Test**: `dotnet test ManagedOnly.slnf`

### Docker

Docker is required for Linux/macOS since RakNet 3.611 requires GCC 4.8.

- **C++ Build**: `just build-cpp`
- **C++ Test**: `just test-cpp`
- **C# Build**: `just build-dotnet`
- **C# Test**: `just test-dotnet`

## Git Workflow

Branch names should follow the format: `{type}/[{gh-issue-number}-]{short-summary}`

Components:
- `{type}`: One of the defined types below
- `{gh-issue-number}` (optional): The GitHub issue number that this branch is addressing.
- `{short-summary}`: A kebab-case short summary of the changes to be made in the branch.
    - GitHub issue branches should match the title, or if it's uncomfortably long, a shortened version that maintains the intent of the original title.

Types:
- **feat**: New functionality
- **fix**: Bug fix
- **refactor**: Restructuring without behavior change
- **chore**: Tooling, config, dependencies, CI
- **docs**: Documentation only

## Helpful Reminders

- The `gh` CLI tool automatically makes inferences about the current repository and active pull request.

## Comprehensive Documentation

- [Architecture](/docs/architecture.md): Contains _incredibly_ detailed information about how packets pass through the system, the threading model of the servers, routing, persistence, and other low-level details. This should be referenced only when a deep-dive into the system is necessary to orchestrate more complicated systems.
- [Adding Packet Handlers](/docs/adding-packet-handlers.md): Step-by-step guide for adding new packet types and handlers across both native and managed code.
