# WorldUpdate Wire Format

The exact RakNet `BitStream` encoding produced by `FOM::Types::WorldUpdate::Write`
— i.e. the body of an `ID_UPDATE` packet (see [[Player Update Flow]]). This is the
bit-packed form on the wire, *not* the 200-byte in-memory [[WorldUpdate]] struct.
All fields are written **little-endian, LSB-first** (byte-reversed first only when
`BitStream::DoEndianSwap()` is true, i.e. on big-endian hosts).

Functions (all `CShell.dll`):

| Function | rva | Role |
| --- | --- | --- |
| `WorldUpdate::Write` | `0x1a1440` | writes `type`, dispatches by variant |
| `WorldUpdate::WritePlayer` | `0x1a1310` | player header, then calls WriteCharacter |
| `WorldUpdate::WriteCharacter` | `0x1a00b0` | id, position, avatar, combat/emote state |
| `PositionRotation::Write` | `0x26be40` | position + 9-bit yaw |
| `PositionRotation::WritePosition` | `0x272420` | quantized x/y/z |
| `Player::Avatar::Write` | `0x2575d0` | appearance + equipment |

Reproduce: `python3 tools/re/fomre.py decompile CShell.dll:0x101a1440` (etc.).

## Encoding primitives

- **`WriteCompressed(v, N, unsigned)`** — RakNet's variable-length form: drops
  leading all-zero bytes, so small values cost few bits. Used for `type`, the
  grids, and ids.
- **`WriteCompressed_T<uint/ushort>`** — same, templated per width.
- **`WriteBits(v, N)`** — exactly `N` raw low bits.
- **`Write1()` / `Write0()`** — a single presence/flag bit. The pervasive pattern
  is *"1 + payload, or 0"* — optional fields cost 1 bit when absent.

## Top level — `Write`

1. `type` — `WriteCompressed(8)`. `1` = player, `2` = character, `3`/`4` other.
   The client only sends **type 1** (see [[Player Update Flow]]).
2. dispatch → `WritePlayer` (type 1).

## Player header — `WritePlayer`

| # | Field | Encoding |
| --- | --- | --- |
| 1 | `grid1` | compressed u32 |
| 2 | `grid2` | compressed u32 |
| 3 | `visibilityAreaId` | compressed, 8 bits (low byte) |
| 4 | `targetingTurretId` | 1 bit; if set → compressed u32 |
| 5 | `activeMedicalTreatment` | 1 bit; if set → 3 bits |
| 6 | `Unknown1` (`field@0xb4`) | compressed u32 — server calls this `Unknown1` |
| 7 | *(character body)* | → `WriteCharacter` |

## Character body — `WriteCharacter`

| # | Field | Encoding | Notes |
| --- | --- | --- | --- |
| 1 | `id` | compressed u32 | player id |
| 2 | `position` | `PositionRotation::Write` | see below |
| 3 | `avatar` | `Avatar::Write` | see below |
| 4 | `isDead` | 1 bit | **gates everything below** |
| 5 | `verticalLookAngle` | 8 bits, written as `angle + 0x5a` (+90) | pitch, biased to unsigned |
| 6 | `animationId` | `0` bit if `==0x10`; else `1` + 12 bits | `0x10` is the idle default |
| 7 | `movementStateId` | `0` bit if 0; else `1` + 5 bits | |
| 8 | `equippedWeapon` | `0` bit if 0; else `1` + compressed u16, then ↓ | unarmed = single 0 bit |
| 8a | ↳ `isWeaponAimed` | 1 bit | only if weapon present |
| 8b | ↳ `consumedAmmo` | `0` bit if 0; else `1` + 7 bits | only if weapon present |
| 8c | ↳ `firedPosition` | `WritePosition` (no yaw) | only if `consumedAmmo != 0` |
| 9 | `wasHit` | 1 bit; if set → `hitAnimationId` 4 bits + `hitDirection` 4 bits | |
| 10 | `emoteId` | `0` bit if 0; else `1` + 6 bits | |
| 11 | *implant block* | present iff avatar has any extended attachment (`slotHat`..`slotHands`) | see Read-path section |
| 11a | ↳ `activeImplants` | `0` bit if 0; else `1` + compressed u16 | |
| 11b | ↳ `shieldSetting` | 7 bits | only if the active implant's `ItemDefinition+0x68` says it supports a shield |
| 12 | `movementSpeed` | 8 bits | |
| 13 | `field@0xb0` | 3 bits | server `Unknown2`; semantics unverified |
| 14 | `field@0xb8` | 1 bit (boolean flag) | server `Unknown3`; semantics unverified |
| 15 | `field@0xbc` | 10 bits | server `Unknown4`; semantics unverified |
| 16 | `field@0xc0` | 10 bits | server `Unknown5`; semantics unverified |
| 17 | `isShieldActive` | 1 bit | last field |

> When `isDead` is true, fields 5–17 are **omitted entirely** — a dead-player
> update is just header + id + position + avatar + the dead bit.

## Position — `PositionRotation::Write` / `WritePosition`

[[PositionRotation]] writes `WritePosition`, then `rot` as **9 bits** (packed yaw).

`WritePosition` branches on `pos.precision` (a *bit width*, not transmitted here —
both ends must agree on it out of band, presumably from world/grid context —
**unverified how the reader obtains it**):

- `precision <= 15` (the common path): for each axis write **|value|** in
  `precision` bits (magnitude via two's-complement abs), then **3 sign bits**
  (x, y, z; `1` = negative). So a position costs `3*precision + 3` bits.
- `precision >= 16`: fall back to a full per-axis compressed write
  (`FUN_1010ee30`, *unverified* — likely `WriteCompressed` of the raw i16).

See [[PositionRotation]] for the in-memory layout. Coordinates are quantized
signed integers, not floats.

## Avatar — `Avatar::Write`

Written in this order (bit widths in parens):

`sex`(1), `skinColor`(1), `face`(5), `hair`(5), `factionId`(**32**), `rankId`(5),
`field@0xc`(6), `legacyFaction`(4), `slotShirt`(12), `slotBottoms`(12),
`slotShoes`(12).

Then an **extended-equipment flag**: if any of the extended slots (`slotHat`
through `slotHands`) is non-zero → write `1` followed by 9 × 12-bit slots
(`slotHat, slotHead, slotEyes, slotShoulder, slotArms, slotTorso, slotBack,
slotLegs, slotHands`); otherwise a single `0` bit and they're skipped.

Finally four 1-bit flags: `isCommander`, `field@0x2a`, `field@0x2c`,
`isGroupLeader`.

> `Avatar::Write` references fields beyond what [[Avatar]] currently documents
> (`slotHands`, `isCommander`, `field@0x2a/0x2c`, `isGroupLeader`) — the in-memory
> [[Avatar]] note covers the type-DB layout; this section is the serialization order.

## Read path & precision sourcing

The deserializer is symmetric with `Write` and lives in **`Object.lto`** (the
gameplay object module — so this is how the client decodes *other* entities'
updates relayed by the world server, not just a server concern):

| Function | rva (Object.lto) | Role |
| --- | --- | --- |
| `WorldUpdate::Read` | `0x51950` | reads `type` (compressed 8), switches: 1→ReadPlayer, 2→ReadCharacter, 3→`FUN_1004eef0`, 4→`FUN_1004f000` |
| `WorldUpdate::ReadPlayer` | `0x518c0` | mirrors `WritePlayer` exactly |
| `WorldUpdate::ReadCharacter` | `0x4ec50` | mirrors `WriteCharacter` exactly |
| `PositionRotation::ReadPosition` | `0xe1ff0` | quantized x/y/z |

Decompiling these **confirms the write-side encoding field-for-field** (every
presence bit, bit width, and order above matches), so the table is verified
bidirectionally.

### `precision` is NOT on the wire — it is caller-supplied (verified)

`ReadPosition` takes the bit width straight from the destination struct:

```c
numberOfBitsToRead = this->pos.precision;   // from the Position object, not the stream
if (precision > 15) { x = ReadCompressed_ushort; y = …; z = …; }   // fallback
else { ReadBits(x, precision); ReadBits(y, precision); ReadBits(z, precision);
       if (ReadBit()) x = -x;  if (ReadBit()) y = -y;  if (ReadBit()) z = -z; }  // 3 sign bits
```

So **`precision` is never serialized** — both peers must set `pos.precision` on
the `Position` object *before* (de)serializing. The values are fixed by the
`WorldUpdate` constructor (verified):

- **main `position` → precision 16.** `PositionRotation`'s default ctor
  (`FUN_1026c0f0`) sets `pos.precision = 0x10`; nothing in `FillUpdate`/`Reset`
  changes it. So `position` takes the `> 15` branch — each axis as a compressed
  16-bit value (`WriteCompressed`/`ReadCompressed`), then 9-bit yaw.
- **`firedPosition` → precision 9.** The `WorldUpdate` ctor explicitly sets
  `firedPosition.precision = 9`, so it takes the `<= 15` branch — 9 magnitude
  bits per axis + 3 sign bits, no yaw.

These look like in-cell offsets (the grid fields locate the cell), which is why 9
bits suffice for `firedPosition`. The server mirrors these exactly in
`WorldUpdateSerializer` (position 16, firedPosition 9); a mismatch here (server
read `firedPosition` at 16) was half of the `ID_UPDATE` ReadError desync, now
fixed.

### Implant / shield gating (refines the table above)

The two gates row 11 marked unverified are now resolved:
- **Implant block presence** is gated by whether the avatar has any *extended
  attachment* (slots `slotHat`..`slotHands`). The write side checks this with
  `FUN_102575b0(&avatar)` (a loop over the 9 extended slots); the read side uses
  `FUN_100c8b60(&avatar)` — the same predicate, and identical to the
  attachment-block gate in [[Avatar]]. If any extended slot is set, the block is
  present: a 1-bit `activeImplants` flag, then the compressed value if non-zero.
- **`shieldSetting`** (7 bits) is present only when the *active implant* supports
  a shield: the client looks the implant up in [[Item Definitions]]
  (`g_ItemDefTable`) and checks `ItemDefinition` field `+0x68`. With no active
  implant (`activeImplants == 0`) it is absent.

> Getting the implant gate wrong was the other half of the `ID_UPDATE` desync:
> the server originally stubbed the block to "never present", skipping a bit the
> client had written, which shifted every following field. The server now mirrors
> the attachment predicate (`HasImplantData`).

Reproduce:

```bash
fomre decompile Object.lto:0x10051950   # Read (dispatch)
fomre decompile Object.lto:0x100518c0   # ReadPlayer
fomre decompile Object.lto:0x1004ec50   # ReadCharacter
fomre decompile Object.lto:0x100e1ff0   # ReadPosition
```

## Server side

The managed deserializer mirrors this in
[`server-shared/Core/Packets/Types/WorldUpdate.cs`](../../server-shared/Core/Packets/Types/WorldUpdate.cs);
its `PlayerUpdate` field order (`Grid1, Grid2, VisibilityAreaId,
TargetingTurretId, ActiveMedicalTreatment, Unknown1, Character`) matches
`WritePlayer` exactly, confirming `field@0xb4` = `Unknown1`.
