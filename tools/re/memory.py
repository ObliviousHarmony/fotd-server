"""Live memory access for the Face of Mankind client running under Wine/Proton.

The Windows client runs as an ordinary Linux process under Wine, so its PE
modules (fom_client.exe, CShell.dll, Object.lto) show up as file-backed
mappings in ``/proc/<pid>/maps``. We locate the process by those mappings,
recover each module's load base, and read/scan memory through
``/proc/<pid>/mem``.

Requires read access to the target's memory: same UID and
``kernel.yama.ptrace_scope = 0`` (or running as a parent of the target). The
functions raise ``MemoryAccessError`` with actionable guidance on EACCES/EPERM.
"""

from __future__ import annotations

import os
import re
import struct
from dataclasses import dataclass

MODULE_NAMES = ("fom_client.exe", "CShell.dll", "Object.lto")

_MAPS_RE = re.compile(
    r"^([0-9a-f]+)-([0-9a-f]+)\s+(\S+)\s+([0-9a-f]+)\s+\S+\s+\d+\s*(.*)$"
)


class MemoryAccessError(RuntimeError):
    pass


class ProcessNotFoundError(RuntimeError):
    pass


@dataclass
class MapRegion:
    start: int
    end: int
    perms: str
    path: str

    @property
    def size(self) -> int:
        return self.end - self.start

    @property
    def readable(self) -> bool:
        return "r" in self.perms

    @property
    def writable(self) -> bool:
        return "w" in self.perms


def read_maps(pid: int) -> list[MapRegion]:
    try:
        text = open(f"/proc/{pid}/maps", "r").read()
    except FileNotFoundError as e:
        raise ProcessNotFoundError(f"no such pid {pid}") from e
    except PermissionError as e:
        raise MemoryAccessError(f"cannot read maps of pid {pid}: {e}") from e
    regions = []
    for line in text.splitlines():
        m = _MAPS_RE.match(line)
        if not m:
            continue
        start, end, perms, _off, path = m.groups()
        regions.append(MapRegion(int(start, 16), int(end, 16), perms, path.strip()))
    return regions


def find_client_pids() -> list[int]:
    """Return PIDs whose mappings reference any known client module."""
    wanted = {n.lower() for n in MODULE_NAMES}
    hits = []
    for entry in os.listdir("/proc"):
        if not entry.isdigit():
            continue
        pid = int(entry)
        try:
            text = open(f"/proc/{pid}/maps", "r").read()
        except (FileNotFoundError, PermissionError, ProcessLookupError):
            continue
        low = text.lower()
        if any(name in low for name in wanted):
            hits.append(pid)
    return sorted(hits)


def find_client_pid() -> int:
    pids = find_client_pids()
    if not pids:
        raise ProcessNotFoundError(
            "no running FOM client found (looked for mappings of "
            f"{', '.join(MODULE_NAMES)}). Launch the client first."
        )
    return pids[0]


def module_base(pid: int, module_name: str) -> int:
    """Lowest mapped address of ``module_name`` = its load base."""
    target = module_name.lower()
    bases = [
        r.start for r in read_maps(pid)
        if os.path.basename(r.path).lower() == target
    ]
    if not bases:
        raise ProcessNotFoundError(
            f"module {module_name!r} not mapped in pid {pid}"
        )
    return min(bases)


def module_bases(pid: int) -> dict[str, int]:
    out: dict[str, int] = {}
    for r in read_maps(pid):
        base = os.path.basename(r.path)
        for name in MODULE_NAMES:
            if base.lower() == name.lower():
                out[name] = min(r.start, out.get(name, r.start))
    return out


def read_mem(pid: int, addr: int, size: int) -> bytes:
    try:
        fd = os.open(f"/proc/{pid}/mem", os.O_RDONLY)
    except PermissionError as e:
        raise _perm_error(pid, e) from e
    except FileNotFoundError as e:
        raise ProcessNotFoundError(f"no such pid {pid}") from e
    try:
        try:
            return os.pread(fd, size, addr)
        except (PermissionError, OSError) as e:
            raise _perm_error(pid, e, addr) from e
    finally:
        os.close(fd)


def write_mem(pid: int, addr: int, data: bytes) -> int:
    """Write ``data`` at ``addr``. Guarded behind FOTD_RE_ALLOW_WRITE=1."""
    if os.environ.get("FOTD_RE_ALLOW_WRITE") != "1":
        raise MemoryAccessError(
            "memory writes are disabled; set FOTD_RE_ALLOW_WRITE=1 to enable"
        )
    fd = os.open(f"/proc/{pid}/mem", os.O_WRONLY)
    try:
        return os.pwrite(fd, data, addr)
    except (PermissionError, OSError) as e:
        raise _perm_error(pid, e, addr) from e
    finally:
        os.close(fd)


def _perm_error(pid: int, e: Exception, addr: int | None = None) -> MemoryAccessError:
    where = f" at {addr:#x}" if addr is not None else ""
    scope = "?"
    try:
        scope = open("/proc/sys/kernel/yama/ptrace_scope").read().strip()
    except OSError:
        pass
    return MemoryAccessError(
        f"cannot access memory of pid {pid}{where}: {e}. "
        f"ptrace_scope={scope}; need same-UID + ptrace_scope=0 "
        f"(sudo sysctl kernel.yama.ptrace_scope=0), or run as the target's parent."
    )


# -- scanning --------------------------------------------------------------
# A scan region is sane to search if it's readable, writable (game state lives
# in rw- heap/data), and not a special kernel mapping.
_SKIP_PATHS = ("[vvar]", "[vdso]", "[vsyscall]")


def scannable_regions(pid: int) -> list[MapRegion]:
    out = []
    for r in read_maps(pid):
        if not (r.readable and r.writable):
            continue
        if r.path in _SKIP_PATHS:
            continue
        out.append(r)
    return out


def encode_value(value, ctype: str) -> bytes:
    fmt = {
        "u8": "<B", "i8": "<b", "u16": "<H", "i16": "<h",
        "u32": "<I", "i32": "<i", "u64": "<Q", "i64": "<q",
        "f32": "<f", "f64": "<d",
    }[ctype]
    return struct.pack(fmt, value)


def scan(pid: int, needle: bytes, *, limit: int = 10000) -> list[int]:
    """Return absolute addresses where ``needle`` occurs in scannable memory."""
    found: list[int] = []
    for region in scannable_regions(pid):
        try:
            buf = read_mem(pid, region.start, region.size)
        except (MemoryAccessError, OSError):
            continue
        off = buf.find(needle)
        while off != -1:
            found.append(region.start + off)
            if len(found) >= limit:
                return found
            off = buf.find(needle, off + 1)
    return found


def scan_value(pid: int, value, ctype: str, *, limit: int = 10000) -> list[int]:
    return scan(pid, encode_value(value, ctype), limit=limit)


def refine(pid: int, addrs: list[int], value, ctype: str) -> list[int]:
    """Keep only addresses that currently hold ``value`` (iterative scanning)."""
    needle = encode_value(value, ctype)
    n = len(needle)
    kept = []
    for a in addrs:
        try:
            if read_mem(pid, a, n) == needle:
                kept.append(a)
        except (MemoryAccessError, OSError):
            continue
    return kept
