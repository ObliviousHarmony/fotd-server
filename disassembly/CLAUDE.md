# Face of Mankind 1.8.5.3 Disassembly

This folder contains the Ghidra disassembly project for the defunct MMOFPS "Face of Mankind". The goal of this project is to explore and document the game's packet structures as Ghidra Data Types in order to aid in the replication of the game's wire format. This will aid in the development of a server emulator by greatly simplifying the process and avoid needing to discover the purpose of every packet through trial and error that inevitably crashes the client. It has been nearly a decade, however, I am the game's original developer and no longer have access to the source code. While I may not remember specifics, I have a deep understanding of how the game works and if I say something about how something was or wasn't done, often, I am going to be correct and would prefer to not continue down lines of thinking that dispute that unless you are _convinced_ I am on the wrong track.

## Client Structure

Face of Mankind is built using the Lithtech Jupiter engine and makes use of RakNet 3.611 as the networking library to interact with the game's servers. The game was compiled using Visual Studio 2008 and the same RakNet static library was linked into all of the game's binaries as well as the servers. The game is comprised of a series of binaries that make up the client:

- SharedLib (`SharedLib.lib`): A static library that was linked into the game client as well as the server. This contained shared enums, item definitions, and other common functionality. For our purposes, the primary utility of this library is the inclusion of the various packet structures. I do _not_ have access to this but it will be a common thread in disassembly across the other binaries and should be kept in mind when common patterns are discovered relating to those elements of the game.
- Game Engine (`fom_client.exe`): The main executable contains only the game engine and provides globals to the other binaries in order to implement game logic. This is where RakNet lives and the main packet receive and send loops are contained within. The other binaries are able to interact with the `g_pLTNetwork` (`ILTNetwork` Type) global for common networking behavior such as logging in, string decoding and encoding using RakNet's `StringCompressor` class (Huffman Encoding Tree), and sending packets. The client dispatches packets to handlers in the other binaries where the game's logic is implemented.
- ClientShell (`CShell.dll`): Imagine this as the window through which the player is able to see and interact with the game's world simulation. This is primarily responsible for the game's user interface and all of the various interactions that take place as a result of this.
- ServerShell (`Object.lto`): This binary is responsible for the actual game world simulation. **This has no relation to the game's actual server!** It handles all of the game objects, physics, particles, and anything else that the player interacts with within the game world. This includes things like the combat system (hit registration, damage animations, grenades, etc) as well as the various terminal objects that the player interacts with. In the latter case, this would mean the player pressing a button while looking at a terminal and then it opening a user interface in the ClientShell.

### Notes

- The ClientShell and ServerShell communicate using an internal messaging system included in the Lithtech Jupiter engine.
- There is a shared memory region between both the ClientShell and ServerShell that is responsible for sharing data between the two without needing to synchronize with messages. This is accessed using the various `SharedMemory::Read` and `SharedMemory::Write` functions. Keep in mind there are duplicates of these functions in disassembly for some reason and you may need to check xrefs for all of them to find what you're looking for.

## Packet Structures

The [`docs/packet-structures`](./docs/packet-structures/) directory contains files for all of the packets within the game. They are located in sub-folders according to where the packet structures and handlers/senders live within the different binaries of the game (`game-engine`, `client-shell`, `server-shell`, and `common`). These files include the in-progress packet structures, constructors, as well as the `Read` and `Write` methods.

### Notes

- The `Read` and `Write` methods provided in packet documentation are Ghidra disassembly output, not original source code.
- Packet structures use auto-packing. Padding is implicit and represented in the field offsets rather than as explicit padding fields.

## String Table Lookups

You have been provided with a [`string-table` skill](/.claude/skills/string-table/SKILL.md) that contains the game's localization string resources in a digestable format. You can use this to perform forward searches for string resources and reverse searches for code that uses a string resource.
