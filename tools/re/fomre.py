#!/usr/bin/env python3
"""fomre — Face of Mankind reverse-engineering harness CLI.

Bridges the committed static symbol/type database (``disassembly/``) to the
live client process running under Wine/Proton.

Examples
--------
  ./fomre.py programs                       # modules + image bases
  ./fomre.py sym Player                      # search symbols
  ./fomre.py sym FillUpdate --exact          # exact name -> address/RVA
  ./fomre.py type ItemDefinition             # struct layout (field offsets)
  ./fomre.py pid                             # find running client + bases
  ./fomre.py read ItemDef_106 --type u32     # read a global as u32 (live)
  ./fomre.py read CShell.dll:0x1030dff0 --type /SharedLib/FOM/Types/Item/ItemDefinition
  ./fomre.py struct CShell.dll:0x1030dff0 /SharedLib/FOM/Types/Item/ItemDefinition
  ./fomre.py scan u32 100                     # find addresses holding 100
"""

from __future__ import annotations

import argparse
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))

import memory  # noqa: E402
import symdb  # noqa: E402


def _db() -> symdb.SymbolDb:
    return symdb.SymbolDb().load()


def _resolve_target(db: symdb.SymbolDb, target: str) -> tuple[str, int]:
    """Return (program, absolute_in_image_addr) from a symbol name or prog:addr."""
    if ":" in target and not target.lower().endswith((".dll", ".exe", ".lto")):
        prog, _, addr_s = target.partition(":")
        return prog, int(addr_s, 0)
    matches = db.resolve(target)
    if not matches:
        sub = db.search(target)
        hint = ""
        if sub:
            hint = "\nDid you mean:\n  " + "\n  ".join(str(s) for s in sub[:10])
        raise SystemExit(f"symbol not found: {target}{hint}")
    if len({(m.program, m.addr) for m in matches}) > 1:
        listed = "\n  ".join(str(s) for s in matches)
        raise SystemExit(f"ambiguous symbol {target!r}, matches:\n  {listed}")
    m = matches[0]
    return m.program, m.addr


def _live_addr(db: symdb.SymbolDb, pid: int, program: str, addr: int) -> int:
    base = memory.module_base(pid, program)
    return base + (addr - db.image_base(program))


def _hexdump(data: bytes, base: int = 0) -> str:
    lines = []
    for i in range(0, len(data), 16):
        chunk = data[i:i + 16]
        hexs = " ".join(f"{b:02x}" for b in chunk)
        asci = "".join(chr(b) if 32 <= b < 127 else "." for b in chunk)
        lines.append(f"{base + i:08x}  {hexs:<47}  {asci}")
    return "\n".join(lines)


# -- commands --------------------------------------------------------------
def cmd_programs(args):
    db = _db()
    for prog, base in sorted(db.programs.items()):
        n = sum(1 for s in db.all_symbols() if s.program == prog)
        print(f"{prog:<16} imageBase={base:#010x}  symbols={n}")


def cmd_sym(args):
    db = _db()
    syms = db.resolve(args.query) if args.exact else db.search(
        args.query, program=args.program, kind=args.kind)
    if not syms:
        raise SystemExit(f"no symbols matching {args.query!r}")
    for s in sorted(syms, key=lambda x: (x.program, x.addr))[:args.limit]:
        print(s)
    if not args.exact and len(syms) > args.limit:
        print(f"... {len(syms) - args.limit} more (raise --limit)")


def cmd_type(args):
    db = _db()
    doc = db.get_type(args.path)
    if doc is None:
        cands = db.find_type(args.path)
        if len(cands) == 1:
            doc = db.get_type(cands[0])
        elif cands:
            print("matches:")
            for c in cands[:40]:
                print(" ", c)
            return
        else:
            raise SystemExit(f"no type matching {args.path!r}")
    kind = doc.get("kind")
    print(f"{doc['path']}  kind={kind} len={doc.get('len')} packed={doc.get('packed')}")
    if kind == "enum":
        for e in doc.get("entries", []) or []:
            cm = f"  // {e['comment']}" if e.get("comment") else ""
            print(f"  {e['value']:>6} {e['name']}{cm}")
        return
    for fld in symdb.iter_fields(doc):
        t = fld.get("type")
        ts = t if isinstance(t, str) else (f"ptr->{t['ptr']}" if "ptr" in t
              else f"{t['arr']}[{t['n']}]" if "arr" in t else str(t))
        cm = f"  // {fld['comment']}" if fld.get("comment") else ""
        print(f"  +{fld['offset']:#06x} ({fld['len']:>4}b) {str(fld.get('name')):<24} {ts}{cm}")


def cmd_pid(args):
    pids = memory.find_client_pids()
    if not pids:
        raise SystemExit("no running FOM client found (launch it first)")
    for pid in pids:
        print(f"pid {pid}")
        for name, base in sorted(memory.module_bases(pid).items()):
            print(f"  {name:<16} base={base:#010x}")


def cmd_maps(args):
    pid = args.pid or memory.find_client_pid()
    for r in memory.read_maps(pid):
        import os
        if os.path.basename(r.path).lower() in {n.lower() for n in memory.MODULE_NAMES}:
            print(f"{r.start:#010x}-{r.end:#010x} {r.perms} {r.path}")


def _pid(args) -> int:
    return args.pid or memory.find_client_pid()


def cmd_read(args):
    db = _db()
    pid = _pid(args)
    prog, addr = _resolve_target(db, args.target)
    live = _live_addr(db, pid, prog, addr)
    t = args.type
    if t and t.startswith("/"):
        return _print_struct(db, pid, prog, addr, live, t)
    if t == "str":
        raw = memory.read_mem(pid, live, args.count)
        s = raw.split(b"\x00", 1)[0]
        print(f"{prog}!{addr:#x} -> {live:#x}: {s.decode('latin-1')!r}")
        return
    if t in ("u8", "i8", "u16", "i16", "u32", "i32", "u64", "i64", "f32", "f64", "ptr"):
        size = {"u8": 1, "i8": 1, "u16": 2, "i16": 2, "u32": 4, "i32": 4,
                "u64": 8, "i64": 8, "f32": 4, "f64": 8, "ptr": 4}[t]
        raw = memory.read_mem(pid, live, size)
        ref = {"ptr": {"ptr": "/void"}}.get(t, "/" + {
            "u8": "uint8_t", "i8": "int8_t", "u16": "uint16_t", "i16": "int16_t",
            "u32": "uint32_t", "i32": "int32_t", "u64": "uint64_t",
            "i64": "int64_t", "f32": "float", "f64": "double"}.get(t, "uint32_t"))
        val = symdb.decode_scalar(ref, raw)
        print(f"{prog}!{addr:#x} -> {live:#x}: {val}")
        return
    raw = memory.read_mem(pid, live, args.count)
    print(f"{prog}!{addr:#x} -> {live:#x} ({args.count} bytes):")
    print(_hexdump(raw, live))


def cmd_struct(args):
    db = _db()
    pid = _pid(args)
    prog, addr = _resolve_target(db, args.target)
    live = _live_addr(db, pid, prog, addr)
    _print_struct(db, pid, prog, addr, live, args.type)


def _print_struct(db, pid, prog, addr, live, type_path):
    doc = db.get_type(type_path)
    if doc is None:
        cands = db.find_type(type_path)
        raise SystemExit(
            f"unknown type {type_path!r}" +
            ("\ncandidates:\n  " + "\n  ".join(cands[:20]) if cands else ""))
    size = doc.get("len") or 0
    raw = memory.read_mem(pid, live, size)
    print(f"{prog}!{addr:#x} -> {live:#x}  {doc['path']} ({size} bytes)")
    for fld in symdb.iter_fields(doc):
        off, ln = fld["offset"], fld["len"]
        cell = raw[off:off + ln]
        t = fld.get("type")
        # enum-aware: show "<int> (NAME)" for enum-typed fields
        enum_name = db.enum_value_name(t, int.from_bytes(cell, "little")) \
            if isinstance(t, str) and cell else None
        if enum_name is not None:
            val = f"{int.from_bytes(cell, 'little')} ({enum_name})"
        else:
            val = symdb.decode_scalar(t, cell)
            if isinstance(val, (bytes, bytearray)):
                val = val.hex()
        print(f"  +{off:#06x} {str(fld.get('name')):<24} = {val}")


def cmd_scan(args):
    pid = _pid(args)
    value = float(args.value) if args.ctype in ("f32", "f64") else int(args.value, 0)
    addrs = memory.scan_value(pid, value, args.ctype, limit=args.limit)
    print(f"{len(addrs)} match(es) for {args.ctype}={args.value} in pid {pid}")
    for a in addrs[:args.show]:
        print(f"  {a:#012x}")
    if len(addrs) > args.show:
        print(f"  ... {len(addrs) - args.show} more")


def main(argv=None):
    p = argparse.ArgumentParser(prog="fomre", description=__doc__,
                                formatter_class=argparse.RawDescriptionHelpFormatter)
    sub = p.add_subparsers(dest="cmd", required=True)

    sub.add_parser("programs", help="list modules and image bases").set_defaults(fn=cmd_programs)

    sp = sub.add_parser("sym", help="search/resolve symbols")
    sp.add_argument("query")
    sp.add_argument("--exact", action="store_true", help="exact name / Ns::Name")
    sp.add_argument("--program")
    sp.add_argument("--kind", choices=["function", "data"])
    sp.add_argument("--limit", type=int, default=40)
    sp.set_defaults(fn=cmd_sym)

    sp = sub.add_parser("type", help="show a type layout")
    sp.add_argument("path", help="full type path or substring")
    sp.set_defaults(fn=cmd_type)

    sub.add_parser("pid", help="find running client + module bases").set_defaults(fn=cmd_pid)

    sp = sub.add_parser("maps", help="show client module mappings")
    sp.add_argument("--pid", type=int)
    sp.set_defaults(fn=cmd_maps)

    sp = sub.add_parser("read", help="read live memory at a symbol or prog:addr")
    sp.add_argument("target", help="symbol name, or prog:0xADDR")
    sp.add_argument("--type", help="u8/i8/u16/i16/u32/i32/u64/i64/f32/f64/ptr/str or a /Type/Path")
    sp.add_argument("--count", type=int, default=64, help="bytes for raw/str reads")
    sp.add_argument("--pid", type=int)
    sp.set_defaults(fn=cmd_read)

    sp = sub.add_parser("struct", help="read a typed struct from live memory")
    sp.add_argument("target")
    sp.add_argument("type", help="a /Type/Path")
    sp.add_argument("--pid", type=int)
    sp.set_defaults(fn=cmd_struct)

    sp = sub.add_parser("scan", help="scan live memory for a value")
    sp.add_argument("ctype", choices=["u8", "i8", "u16", "i16", "u32", "i32",
                                      "u64", "i64", "f32", "f64"])
    sp.add_argument("value")
    sp.add_argument("--pid", type=int)
    sp.add_argument("--limit", type=int, default=100000)
    sp.add_argument("--show", type=int, default=20)
    sp.set_defaults(fn=cmd_scan)

    args = p.parse_args(argv)
    args.fn(args)


if __name__ == "__main__":
    main()
