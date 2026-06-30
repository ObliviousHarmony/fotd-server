# `fomre` — Reverse-engineering harness

Tooling that makes the FOM client's internals queryable from the command line:
it reads the committed static RE database under [`disassembly/`](../../disassembly)
and bridges it to the **live client process** running under Wine/Proton.

No Ghidra and no game binary are needed to use the static half — the symbol and
type JSON already lives in the repo. The live half needs the client running.

## Layout

```
tools/re/
  symdb.py     static symbol + type database over disassembly/*.json (stdlib only)
  memory.py    live process memory: find PID, module bases, read/write/scan via /proc
  fomre.py     CLI tying the two together
  tests/       unittest suite over the committed JSON (no game, CI-safe)
```

## The three questions this answers

**1. What makes the binary accessible from the CLI?**
The hand-built Ghidra analysis is exported to diffable JSON (functions with
addresses/signatures/namespaces, typed globals, struct/enum layouts with field
offsets). `symdb.py` indexes it: resolve a name to an address, list a class's
members, print a struct's layout, map an enum value to its name.

**2. Does Ghidra have a CLI?**
Yes — `analyzeHeadless` and **PyGhidra** (`pyghidra_launcher.py --headless`).
This repo already drives it via `just ghidra-gen` / `just ghidra-dump`
(see [`disassembly/README.md`](../../disassembly/README.md)). Those rebuild/export
the labelled project headless. Ghidra is **not installed on this machine yet**;
to enable on-demand decompilation/xref scripts, install **Ghidra 12.0.4**
(version-pinned) and set `GHIDRA_INSTALL_DIR`.

**3. Application memory values?**
The Windows client runs as a normal Linux process under Wine/Proton, so its PE
modules are file-backed mappings in `/proc/<pid>/maps`. `memory.py` finds the
process, recovers each module's **load base** (modules get relocated — e.g.
CShell.dll loads at `0x763e0000`, not its preferred `0x10000000`), and
reads/scans `/proc/<pid>/mem`. A static symbol's live address is
`module_base + (addr - imageBase)`.

## Usage

```bash
# --- static (no game needed) ---
python3 tools/re/fomre.py programs                      # modules + image bases
python3 tools/re/fomre.py sym Player                     # search symbols
python3 tools/re/fomre.py sym FillUpdate --exact         # name -> addr / RVA
python3 tools/re/fomre.py type ItemDefinition            # struct field layout
python3 tools/re/fomre.py type ItemCategory              # enum members

# --- live (client running under Wine/Proton) ---
python3 tools/re/fomre.py pid                            # find client + module bases
python3 tools/re/fomre.py read CShell.dll:0x103c3fa8 --type ptr
python3 tools/re/fomre.py struct CShell.dll:0x1030dff0 /FOM/Types/Item/ItemDefinition
python3 tools/re/fomre.py scan u32 1000                  # find addresses holding 1000
```

A read/struct target is either a **symbol name** (resolved via the DB) or an
explicit `program:0xADDR` (the in-image address, as stored in the JSON) — use the
latter when a name exists in more than one module.

## Live-memory requirements

Reading another process's memory needs ptrace access:

- **same UID** as the client, **and**
- `kernel.yama.ptrace_scope = 0` (this host is already `0`), or run the harness
  as the client's parent.

On `EACCES`/`EPERM` the tool prints the exact reason and the
`sudo sysctl kernel.yama.ptrace_scope=0` remedy. Writes are **off by default**;
set `FOTD_RE_ALLOW_WRITE=1` to enable `write_mem`.

## Tests

```bash
just re-test          # or:
python3 -m unittest discover -s tools/re/tests
```

The suite validates symbol resolution, RVA math, image bases, struct/enum
layouts, and scalar decoding against the committed JSON — it needs neither
Ghidra nor a running game, so it runs in CI.
