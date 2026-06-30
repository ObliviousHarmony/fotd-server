# Client Knowledge Base

Reverse-engineering notes for the **Face of Mankind** client (build 1.8.5.3),
derived from the committed Ghidra analysis in [`disassembly/`](../../disassembly)
and verified against the live process with the RE harness in
[`tools/re/`](../../tools/re). Everything here is grounded in a decompile, a
symbol/type record, or a live memory read — addresses and reproduction commands
are cited so any claim can be re-checked.

## Start here

**Foundations**
- [[Client Architecture]] — the three binaries and the engine they run on
- [[SharedMemory]] — the in-process state blackboard everything reads from

**Player state → server**
- [[Player Update Flow]] — how the client reports your state to the world server
- [[WorldUpdate]] — the structure that carries that state ([[Avatar]], [[PositionRotation]])
- [[WorldUpdate Wire Format]] — the exact bit-packed `ID_UPDATE` body

**Protocol**
- [[Packet Transport]] — the `VariableSizedPacket` envelope and `SendPacket`
- [[Login Handshake]] — the master-server login + credential hashing
- [[World Login Handoff]] — master-orchestrated transfer into a world server
- [[Character Creation]] — the `CREATE_CHARACTER` packet (appearance only)

**Game data**
- [[Item Definitions]] — the `g_ItemDefTable` item catalog
- [[Inventory]] — item containers and the inventory wire protocol

## How these notes were produced

```bash
python3 tools/re/fomre.py sym <name>            # resolve symbol -> address/RVA
python3 tools/re/fomre.py type <Type>           # struct/enum layout
python3 tools/re/fomre.py decompile <sym>       # decompiled C (Ghidra headless)
python3 tools/re/fomre.py xref <sym>            # callers / callees
python3 tools/re/fomre.py read|struct|scan ...  # live process memory
```

Image bases (a symbol's `addr` is absolute-in-image; `rva = addr - imageBase`):
`CShell.dll` and `Object.lto` = `0x10000000`, `fom_client.exe` = `0x00400000`.
Modules are relocated at load, so live addresses use the runtime base from
`/proc/<pid>/maps`, not the preferred base.

> Server-side counterparts live in the sibling **server** vault — see
> *Server Topology* there for where the client's packets land.
