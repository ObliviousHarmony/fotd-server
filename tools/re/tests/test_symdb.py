"""Tests for the static symbol/type database.

These run against the committed disassembly JSON only — no Ghidra, no binary,
no running game — so they are safe for CI.

Run:  python3 -m unittest discover -s tools/re/tests
"""

import struct
import sys
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))

import symdb  # noqa: E402


class SymbolDbTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.db = symdb.SymbolDb().load()

    def test_programs_and_image_bases(self):
        progs = self.db.programs
        self.assertEqual(progs["CShell.dll"], 0x10000000)
        self.assertEqual(progs["Object.lto"], 0x10000000)
        self.assertEqual(progs["fom_client.exe"], 0x00400000)

    def test_symbols_loaded(self):
        self.assertGreater(len(self.db.all_symbols()), 100)

    def test_resolve_function_and_rva(self):
        # FOM::Player::FillUpdate @ 0x101a2390 in CShell.dll (imageBase 0x10000000)
        matches = self.db.resolve("FillUpdate")
        self.assertTrue(matches)
        fill = next(m for m in matches if m.namespace == "FOM::Player")
        self.assertEqual(fill.program, "CShell.dll")
        self.assertEqual(fill.addr, 0x101A2390)
        self.assertEqual(fill.rva, 0x1A2390)
        self.assertEqual(fill.kind, "function")

    def test_resolve_qualified_name(self):
        matches = self.db.resolve("FOM::Player::FillUpdate")
        self.assertTrue(any(m.addr == 0x101A2390 for m in matches))

    def test_resolve_global_data(self):
        matches = self.db.resolve("ItemDef_106")
        self.assertTrue(matches)
        self.assertEqual(matches[0].kind, "data")
        self.assertEqual(matches[0].addr, 0x1030DFF0)
        self.assertEqual(matches[0].rva, 0x30DFF0)

    def test_search_substring(self):
        hits = self.db.search("ItemDef_", kind="data")
        self.assertGreater(len(hits), 100)

    def test_type_layout_bitstream(self):
        bs = self.db.get_type("/RakNet/BitStream")
        self.assertIsNotNone(bs)
        self.assertEqual(bs["len"], 1044)
        fields = {f["name"]: f for f in symdb.iter_fields(bs)}
        self.assertEqual(fields["numberOfBitsUsed"]["offset"], 0)
        self.assertEqual(fields["data"]["offset"], 12)
        self.assertEqual(fields["stackData"]["offset"], 17)
        self.assertEqual(fields["stackData"]["len"], 1024)
        # pointer field encoded as a dict
        self.assertIn("ptr", fields["data"]["type"])


class DecodeScalarTests(unittest.TestCase):
    def test_unsigned_and_signed(self):
        self.assertEqual(symdb.decode_scalar("/stdint.h/uint32_t",
                                             struct.pack("<I", 0xDEADBEEF)), 0xDEADBEEF)
        self.assertEqual(symdb.decode_scalar("/int", struct.pack("<i", -5)), -5)
        self.assertEqual(symdb.decode_scalar("/ushort", struct.pack("<H", 513)), 513)

    def test_bool_and_float(self):
        self.assertIs(symdb.decode_scalar("/bool", b"\x01"), True)
        self.assertAlmostEqual(symdb.decode_scalar("/float",
                                                   struct.pack("<f", 1.5)), 1.5)

    def test_pointer_dict(self):
        out = symdb.decode_scalar({"ptr": "/void"}, struct.pack("<I", 0x10203040))
        self.assertEqual(out, "0x10203040")

    def test_array_returns_raw(self):
        raw = b"\x00\x01\x02\x03"
        self.assertEqual(symdb.decode_scalar({"arr": "/uint8_t", "n": 4}, raw), raw)

    def test_short_buffer_returns_raw(self):
        # not enough bytes -> raw passthrough, never raises
        self.assertEqual(symdb.decode_scalar("/uint32_t", b"\x01\x02"), b"\x01\x02")


if __name__ == "__main__":
    unittest.main()
