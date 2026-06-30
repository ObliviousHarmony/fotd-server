"""Static symbol/type database for the Face of Mankind client binaries.

Reads the committed reverse-engineering work product under ``disassembly/``
(``symbols/<program>/**/*.json`` and ``types/**/*.json``) and exposes it as a
queryable index. No Ghidra, no binary, and no running game required — this is
pure data over the JSON that already lives in the repo.

Addresses in the symbol JSON are absolute *within the binary's image* (Ghidra's
view), so the file-relative offset is ``rva = addr - imageBase``. Each module
has its own image base (CShell.dll/Object.lto = 0x10000000, fom_client.exe =
0x00400000), recorded in ``symbols/<program>/_meta.json``.
"""

from __future__ import annotations

import json
import os
import struct
from dataclasses import dataclass, field
from pathlib import Path
from typing import Iterable

# tools/re/symdb.py -> repo root is two parents up.
_REPO_ROOT = Path(__file__).resolve().parents[2]
DISASM_DIR = Path(os.environ.get("FOTD_DISASM_DIR", _REPO_ROOT / "disassembly"))

# 32-bit PE binaries: pointers are 4 bytes.
POINTER_SIZE = 4


@dataclass(frozen=True)
class Symbol:
    program: str
    name: str
    namespace: str
    addr: int  # absolute in the binary's image
    kind: str  # "function" | "data"
    image_base: int

    @property
    def rva(self) -> int:
        return self.addr - self.image_base

    @property
    def qualified_name(self) -> str:
        return f"{self.namespace}::{self.name}" if self.namespace else self.name

    def __str__(self) -> str:
        return (
            f"{self.program}!{self.qualified_name} "
            f"@ {self.addr:#010x} (rva {self.rva:#x}) [{self.kind}]"
        )


class SymbolDb:
    """Lazily-loaded index over the disassembly JSON."""

    def __init__(self, disasm_dir: Path | str = DISASM_DIR):
        self.disasm_dir = Path(disasm_dir)
        self.symbols_dir = self.disasm_dir / "symbols"
        self.types_dir = self.disasm_dir / "types"
        self._image_bases: dict[str, int] = {}
        self._symbols: list[Symbol] = []
        self._by_name: dict[str, list[Symbol]] = {}
        self._types: dict[str, dict] = {}  # canonical type path -> type record
        self._loaded = False

    # -- loading -----------------------------------------------------------
    def _ensure(self) -> None:
        if not self._loaded:
            self.load()

    def load(self) -> "SymbolDb":
        if not self.symbols_dir.is_dir():
            raise FileNotFoundError(
                f"symbols dir not found: {self.symbols_dir} "
                f"(set FOTD_DISASM_DIR to override)"
            )
        for prog_dir in sorted(p for p in self.symbols_dir.iterdir() if p.is_dir()):
            self._load_program(prog_dir)
        if self.types_dir.is_dir():
            self._load_types()
        self._loaded = True
        return self

    def _load_program(self, prog_dir: Path) -> None:
        program = prog_dir.name
        meta_path = prog_dir / "_meta.json"
        image_base = 0
        if meta_path.is_file():
            meta = json.loads(meta_path.read_text())
            image_base = int(meta.get("imageBase", "0"), 16)
        self._image_bases[program] = image_base

        for jf in prog_dir.rglob("*.json"):
            if jf.name in ("_meta.json", "namespaces.json"):
                continue
            try:
                doc = json.loads(jf.read_text())
            except json.JSONDecodeError:
                continue
            if isinstance(doc, list):
                # bulk typed globals (e.g. _data/ItemDefinition.json)
                for dt in doc:
                    if isinstance(dt, dict):
                        self._add(program, dt, "data", image_base)
                continue
            if not isinstance(doc, dict):
                continue
            for fn in doc.get("functions", []) or []:
                self._add(program, fn, "function", image_base)
            for dt in doc.get("data", []) or []:
                self._add(program, dt, "data", image_base)

    def _add(self, program: str, entry: dict, kind: str, image_base: int) -> None:
        addr_s = entry.get("addr")
        name = entry.get("name")
        if addr_s is None or name is None:
            return
        sym = Symbol(
            program=program,
            name=name,
            namespace=entry.get("namespace", "") or "",
            addr=int(addr_s, 16),
            kind=kind,
            image_base=image_base,
        )
        self._symbols.append(sym)
        self._by_name.setdefault(name.lower(), []).append(sym)
        if sym.namespace:
            self._by_name.setdefault(sym.qualified_name.lower(), []).append(sym)

    def _load_types(self) -> None:
        for jf in self.types_dir.rglob("*.json"):
            try:
                doc = json.loads(jf.read_text())
            except json.JSONDecodeError:
                continue
            if isinstance(doc, dict) and doc.get("path"):
                self._types[doc["path"]] = doc

    # -- queries -----------------------------------------------------------
    @property
    def programs(self) -> dict[str, int]:
        self._ensure()
        return dict(self._image_bases)

    def image_base(self, program: str) -> int:
        self._ensure()
        return self._image_bases[program]

    def all_symbols(self) -> list[Symbol]:
        self._ensure()
        return list(self._symbols)

    def resolve(self, name: str) -> list[Symbol]:
        """Exact match on name or ``Namespace::Name`` (case-insensitive)."""
        self._ensure()
        return list(self._by_name.get(name.lower(), []))

    def search(self, substring: str, *, program: str | None = None,
               kind: str | None = None) -> list[Symbol]:
        self._ensure()
        needle = substring.lower()
        out = []
        for s in self._symbols:
            if needle not in s.qualified_name.lower():
                continue
            if program and s.program != program:
                continue
            if kind and s.kind != kind:
                continue
            out.append(s)
        return out

    def by_addr(self, program: str, addr: int) -> Symbol | None:
        self._ensure()
        for s in self._symbols:
            if s.program == program and s.addr == addr:
                return s
        return None

    def get_type(self, path: str) -> dict | None:
        self._ensure()
        return self._types.get(path)

    def find_type(self, name_substring: str) -> list[str]:
        self._ensure()
        n = name_substring.lower()
        return sorted(p for p in self._types if n in p.lower())

    def enum_value_name(self, type_doc_or_path, value: int) -> str | None:
        """Map an integer to its enum member name, if the type is an enum."""
        doc = (self.get_type(type_doc_or_path)
               if isinstance(type_doc_or_path, str) else type_doc_or_path)
        if not doc or doc.get("kind") != "enum":
            return None
        for e in doc.get("entries", []) or []:
            if e.get("value") == value:
                return e.get("name")
        return None


# -- scalar decoding -------------------------------------------------------
# Map a Ghidra type token (the part after the leading "/") to a struct format.
_SCALAR_FORMATS: dict[str, str] = {
    "bool": "?",
    "char": "b", "schar": "b", "int8_t": "b", "sbyte": "b",
    "uchar": "B", "byte": "B", "uint8_t": "B", "undefined1": "B",
    "short": "h", "int16_t": "h",
    "ushort": "H", "word": "H", "uint16_t": "H", "undefined2": "H",
    "int": "i", "long": "i", "int32_t": "i",
    "uint": "I", "dword": "I", "ulong": "I", "uint32_t": "I", "undefined4": "I",
    "longlong": "q", "int64_t": "q",
    "ulonglong": "Q", "uint64_t": "Q", "undefined8": "Q",
    "float": "f", "double": "d",
}


def decode_scalar(type_ref, raw: bytes):
    """Decode ``raw`` bytes per a Ghidra type reference (string or dict).

    Returns a Python value (int/float/bool), a hex string for pointers, or the
    raw bytes when the type isn't a recognised scalar (arrays, structs, etc.).
    """
    if isinstance(type_ref, dict):
        if "ptr" in type_ref:
            if len(raw) >= POINTER_SIZE:
                return "0x%08x" % struct.unpack_from("<I", raw)[0]
            return raw
        return raw  # arr / nested aggregate -> caller handles bytes
    token = (type_ref or "").lstrip("/").split("/")[-1].lower()
    fmt = _SCALAR_FORMATS.get(token)
    if fmt and struct.calcsize("<" + fmt) <= len(raw):
        return struct.unpack_from("<" + fmt, raw)[0]
    return raw


def iter_fields(type_doc: dict) -> Iterable[dict]:
    """Yield field dicts (name, offset, len, type) for a struct type doc."""
    return type_doc.get("fields", []) or []
