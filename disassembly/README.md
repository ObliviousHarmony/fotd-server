# Ghidra Annotation Export — Face of Mankind 1.8.5.3

This directory preserves the hand-built Ghidra analysis of the Face of Mankind client binaries
as **diffable, regenerable JSON**, so the labeling lives in version control **without
redistributing the game binary** (which we don't own). You bring your own copy of a binary; one
command rebuilds the fully-labeled Ghidra project on top of a fresh import.

## What's here

```
types/                       one JSON file per data type, organized by bucket then category
  SharedLib/FOM/...            shared types (SharedLib.lib) + the /std container types
  RakNet/...                   RakNet library types
  stdint.h/  crtdefs.h/        external typedefs the binaries reference
  CShell.dll/FOM/Windows/...   a binary's OWN types (e.g. window classes)
symbols/<program>/           the labeling, sharded by namespace
  namespaces.json              the class/namespace tree
  FOM/Windows/CWindowMgr.json  a class: its functions + namespaced data + comments
  _global.json                 functions/data not in a user namespace
  _data/ItemDefinition.json    bulk typed globals (the ~1000 ItemDef_* records)
scripts/
  export_program.py            dump one labeled program -> types/ + symbols/   (PyGhidra)
  build_program.py             reconstruct one program from JSON onto a fresh import
  validate_build.py            re-dump a built program in memory and diff vs the shards
```

The `just ghidra-gen` / `just ghidra-dump` recipes (in the repo's [`justfile`](../justfile)) drive these
scripts headless — one command for the whole project, all binaries.

Each binary contributes its own local type bucket + `symbols/<program>/`; `SharedLib`, `RakNet`,
and the external buckets are shared across all of them.

## What these files are — and aren't

The type JSON describes the *shapes* of the game's data structures (field names, offsets, sizes,
enum values, function prototypes) using structural references (`{"ptr": ...}`, `{"arr": ..., "n": N}`).
The symbol JSON is names, namespace placement, signatures, and comments.

There is **no machine code, no disassembly, no decompiler output, no binary bytes, and no
addresses copied out of the binary**. This is reverse-engineering work product — descriptions of
structure, like documentation. The binary, the Ghidra project (`*.gpr`/`*.rep`), and any `.gzf`
are **never** committed (see [`.gitignore`](../.gitignore)).

## Prerequisites

- **Ghidra 12.0.4** — pinned. The build layers annotations on top of a fresh auto-analysis, so the
  analyzer version must match; the scripts refuse to run otherwise. (The original project was
  authored in 11.4.3; the export is validated to reproduce on 12.0.4.)
- **PyGhidra** (Python 3) — the scripts use it; the `just ghidra-*` recipes launch headless through it.
- **Your own copy** of the game (the binaries), build 1.8.5.3.

## Rebuild the labeled project (one command)

```
just ghidra-gen
```

Ghidra is located via `$GHIDRA_INSTALL_DIR` (default `C:\Program Files (x86)\Ghidra` on Windows,
`/opt/ghidra` elsewhere); your binaries via `$FOTD_GAME_DIR` (default `../client`). The project is
built in place at `disassembly/FOTD`.

It finds each binary under the game directory, and for each: imports it, auto-analyzes, reconstructs
the shared types into a **`SharedLib` project archive inside the project** and links the program to it
(so shared types stay **linked** — syncable in the Data Type Manager, and resolved automatically
whenever the project is open), reconstructs this binary's own types into the program, recreates
your namespaces and relocates the RTTI classes, applies every function name + signature, names/types
the globals, and restores comments — into one project. Then open that project in Ghidra.

The first binary of a build creates the `SharedLib` project archive from the JSON; the rest reuse it,
so all programs link to one shared identity. It lives **inside** the project (`.rep`) — no external
file, no path to lose (which is what caused "archive file not found" with a `.gdt`) — and, like the
project itself, it's never committed (the JSON is the source of truth).

(Under the hood this is `pyghidra_launcher.py --headless ... -postScript build_program.py` per binary.)

## Re-export after more analysis

```
just ghidra-dump             # run with the project CLOSED in the GUI (headless needs the lock)
```

Workflow: label and create types in Ghidra; link the shared ones to the `SharedLib` archive (that
linkage is what the exporter reads to bucket shared-vs-local); run `just ghidra-dump`; commit the JSON
diff. The export captures the full dependency closure, so referenced externals ship too. Shared
buckets accumulate across binaries; a binary's own bucket is rewritten fresh.

## Upgrading Ghidra later

Open the project on the new Ghidra, let it upgrade, re-run `just ghidra-dump` on that version, and bump
`EXPECTED_GHIDRA_VERSION` in the scripts.
