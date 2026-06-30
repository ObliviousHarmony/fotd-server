# Character Creation

What the client sends when you create a character — the flow triggered when the
master returns `CREATE_CHARACTER` at login (see [[Login Handshake]]). The packet
carries **appearance only**: no attributes or skills are transmitted.

## Trigger

A `CREATE_CHARACTER` login result opens `CMenuCharacterCreation` (`CShell.dll`):
`SetupControls` (rva `0x879d0`), `Render` (rva `0x871b0`), `OnEvent`
(rva `0x88960`). The menu gathers appearance + name + biography; on confirm it
builds `Packet_ID_CREATE_CHARACTER` and sends it to the **master** server via
`SendPacket(..., MASTER)` (see [[Packet Transport]]). *(The OnEvent→send wiring
is inferred from the class+packet; the packet and its serializer are verified.)*

## Packet — `Packet_ID_CREATE_CHARACTER` (ID 122)

`fomre type /FOM/Packets/Packet_ID_CREATE_CHARACTER` — 1664 bytes:

| Offset | Field | Type | Notes |
| --- | --- | --- | --- |
| `0x000` | `base` | `VariableSizedPacket` | envelope ([[Packet Transport]]) |
| `0x430` | `playerId` | u32 | the account id, echoed from `ID_LOGIN_RETURN` |
| `0x434` | `avatar` | [[Avatar]] (50B) | appearance + equipment slots |
| `0x466` | `name` | `char[20]` | character name (≤19 + NUL) |
| `0x47a` | `biography` | `char[512]` | freeform bio text |

### Wire serialization — `Write` (rva `0x86620`)

```c
VariableSizedPacket::Write(&base);              // header: ID_CREATE_CHARACTER
WriteCompressed_uint(playerId);
Avatar::Write(&avatar, bitStream);              // bit-packed; see WorldUpdate Wire Format
g_pLTNetwork->EncodeString(name, 0x800, bs);    // length-capped string
g_pLTNetwork->EncodeString(biography, 0x800, bs);
```

So the body is: `playerId` (compressed) → `avatar` (the same bit-packed encoding
documented for [[Avatar]] in [[WorldUpdate Wire Format]]) → `name` → `biography`
(both via the engine's `EncodeString`, the RakNet StringCompressor path, capped
at 0x800 chars).

## Appearance vs. attributes — attributes are NOT client-sent

The packet contains **no attributes, skills, faction, or starting stats** — only
`avatar` (sex/race/face/hair + equipment-slot ids), `name`, and `biography`. The
server's mirror confirms this: `CreateCharacter`
(`server-shared/Core/Packets/CreateCharacter.cs`) is exactly
`{ PlayerId, Avatar, RawName, RawBiography }`.

`CreateCharacterHandler` (master) creates the `player` row from
`PlayerId, Name, Biography, Avatar.Sex/Race/Face/Hair` and nothing else — so the
`player_attribute` table is **not** written during creation.

> **Starting attributes are server-seeded by design, but the seeding is not yet
> implemented.** The server defines `PlayerConstants.AttributeDefaultValues` (e.g.
> Health 1000, Aura 1000, Agility 900, AuraRegeneration 30, JumpVelocityMultiplier
> 1000) indexed by `AttributeType`, but **no code inserts `player_attribute` rows**
> (grep finds no writer). This is exactly why creating "Tom Dev" left
> `player_attribute` empty — it's a server gap, not client behaviour. A future
> server change should seed `AttributeDefaultValues` into `player_attribute` on
> character creation (and/or on first world entry).

## Reproduce

```bash
fomre type /FOM/Packets/Packet_ID_CREATE_CHARACTER
fomre decompile CShell.dll:0x10086620          # Write (serialization)
fomre sym CharacterCreation                     # CMenuCharacterCreation
```
