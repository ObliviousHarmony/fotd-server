
# Face of Mankind Emulator

This repository contains a server emulator for the discontinued MMOFPS, "Face of Mankind", first released in 2006. It features a client/server architecture with two kinds of servers, a "master" server that holds cross-server global state and individual "world" servers for player movement and interaction within a given world.

## Projects

- [RakNet 3.611](/extern/raknet) is the game's underlying network library. **Never modify any file under `/extern/raknet`** — it is vendored third-party code and is exempt from all conventions in this document.
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

- **Acronym casing**: Treat acronyms as words — capitalize only the first letter, regardless of length — then apply the language's normal casing (PascalCase for C# types/members and C++ types, camelCase for C++ members, etc.). Plurals follow the same rule. Examples: `PlayerId`, `WorldId`, `PacketIds`, `ApiStructs`, `HtmlParser`, `Db`, `Ip`.

  The following are **not** acronyms-in-identifiers and are exempt from the rule:

  | Exception | Handling | Examples |
  | --- | --- | --- |
  | `/extern/raknet` | Vendored third-party; never modified at all. | — |
  | `FOM` (the product, "Face of Mankind") | Proper noun — stays uppercase. | `FOMNetwork`, `FOMServer`, `FOMDataSerializer` |
  | In-game item / weapon / skill / apartment names | Proper nouns mirroring the original game's master data — keep their original casing. The `SCREAMING_SNAKE_CASE` wire constant in the trailing comment is authoritative. | `GakkMG6`, `EnfieldERX`, `TacticalHQ`, `SalvotecHP` |
  | `SCREAMING_SNAKE_CASE` constants | A separate naming convention; acronym casing does not apply. Covers the `ID_*` wire identifiers (which mirror RakNet's `MessageIdentifiers`) and enum value names. | `ID_WORLD_LOGIN`, `MASTER_SERVER`, `NUM_WORLDS` |
  | External / framework type names | Keep the upstream spelling; not ours to rename. | `IPAddress`, `IPEndPoint` |

- **No empty property patterns**: Never use the empty property pattern `{ }` for null tests or captures — not `x is { }`, `x is not { }`, nor `x is { } y`. Use `is null` / `is not null` for the check, and `.HasValue` + `.Value` (or a plain assignment) when you need the value; it reads more clearly, and the `{ }` form additionally hides a `Nullable<T>` unwrap. Property patterns with real subpatterns (`x is { Status: Success }`) are fine.

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

### Branch Naming Convention

Branch names should follow the format: `{type}/[{gh-issue-number}-]{short-summary}`

### Components

- `{type}`: One of the defined types below
- `{gh-issue-number}` (optional): The GitHub issue number that this branch is addressing.
- `{short-summary}`: A kebab-case short summary of the changes to be made in the branch.
    - GitHub issue branches should match the title, or if it's uncomfortably long, a shortened version that maintains the intent of the original title.

### Types

- **feat**: New functionality
- **fix**: Bug fix
- **refactor**: Restructuring without behavior change
- **chore**: Tooling, config, dependencies, CI
- **docs**: Documentation only

### Commit Messages

The First Line Should Be in Title Case with a maximum of 50 characters. After an empty line, provide a more detailed explanation with a maximum line length of 72 characters.

Focus on the **why**, not the **what**. The diff shows what changed - commit messages should explain the reasoning, context, and constraints that led to the change. Avoid describing implementation details that are visible in the code.

### Tools

- The `gh` CLI tool automatically makes inferences about the current repository and active pull request.
- The repository contains a pull request template.

## Comprehensive Documentation

- [Architecture](/docs/architecture.md): Contains _incredibly_ detailed information about how packets pass through the system, the threading model of the servers, routing, persistence, and other low-level details. This should be referenced only when a deep-dive into the system is necessary to orchestrate more complicated systems.
- [Adding Packet Handlers](/docs/adding-packet-handlers.md): Step-by-step guide for adding new packet types and handlers across both native and managed code.
