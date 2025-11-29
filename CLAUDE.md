# Face of Mankind Emulator

This repository contains a server emulator for the discontinued MMOFPS, "Face of Mankind", first released in 2006. It features a client/server architecture with two kinds of servers, a "master" server that holds cross-server global state and individual "world" servers for player movement and interaction within a given world.

## Projects

- [RakNet 3.611](/extern/raknet) is the game's underlying network library.
- [FOMNetwork](/fom-network) conmtains native packet definitions, serializers, and abstracts RakNet.
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

- `.editorconfig` formatting rules
- C++: `clang-format`
- C#: `dotnet`

## Common Commands

- `cmake` is provided for building native code and `ctest` is provided for testing.
- `dotnet` with the `ManagedOnly.slnf` filter file is provided for building and testing native code.
- `just build`, `just build-cpp`, `just build-dotnet`, and related `test` commands are provided, however, they use `docker` for compatibility reasons and should be avoided on Windows hosts if possible.
- `just format`, `just format-cpp`, `just format-dotnet`, and related `format-check` commands are provided for linting and formatting.

## Comprehensive Documentation

- [Architecture](/docs/architecture.md): Contains _incredibly_ detailed information about how packets pass through the system, the threading model of the servers, routing, and other low-level details. This should be referenced only when a deep-dive into the system is necessary to orchestrate more complicated systems.
-
