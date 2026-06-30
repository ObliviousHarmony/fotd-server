# Inventory

How the client models items and syncs them with the server. The model has three
layers: an **`ItemBase`** (one item's template + per-item state), an
**`ItemStack`** (an `ItemBase` plus the set of physical instance ids stacked
under it), and an **`ItemList`** (a container's worth of stacks). Each item's
`type` resolves to its catalog record via [[Item Definitions]]
(`g_ItemDefTable[type]`). All wire encoding uses RakNet **compressed** integers
(see the codecs in [[Packet Transport]]).

## The player's containers — `Player::Inventory` (1084 bytes)

`type /FOM/Types/Player/Inventory`:

| Offset | Field | Type | Notes |
| --- | --- | --- | --- |
| `0x000` | `inventory` | `ItemList` | the main backpack |
| `0x024` | `equipment` | `EquipmentSlots` | `Item[12]` (worn gear, 576B) |
| `0x264` | `weapons` | `WeaponSlots` | 148B |
| `0x2f8` | `nanomachines` | `NanomachineSlots` | 288B |
| `0x418` | `storage` | `ItemList` | apartment/locker storage |

So a player has five logical containers. The `src`/`dest` bytes in the item
packets (below) select among them (exact enum **unverified** — no named
`ItemContainer` enum in the symbol DB; the values are small container ids).

The equipment slot element is `Item` (48B) = `{ u32 itemId; ItemBase item; … }` —
i.e. an instance id paired with the item state.

## `ItemBase` (32 bytes) — one item's state

`type /FOM/Types/Item/ItemBase`. Fields and their **wire order** from
`ItemBase::Write` (`CShell.dll` rva `0x254d40`) — every field is
`WriteCompressed`:

| # | Field | Bits (compressed) | Notes |
| --- | --- | --- | --- |
| 1 | `type` | 16 | item type id → [[Item Definitions]] |
| 2 | `value` | 16 | stack count / quantity |
| 3 | `maxDurability` | 16 | |
| 4 | `durability` | 16 | |
| 5 | `durabilityLossFactor` | 8 | |
| 6 | `security` | 8 | `ItemSecurity`: NORMAL/SECURED/BOUND/SPECIAL_BOUND |
| 7 | `creatorPlayerId` | 32 | crafted-by |
| 8 | `timeout` | 32 | expiry |
| 9 | `stolenFromPlayerId` | 32 | |
| 10 | `classification` | 8 | |
| 11 | `quality` | 8 | `ItemQuality` |
| 12 | `attributeBonus` | 8 | |
| 13 | `balanceValues[4]` | 8 each | per-item tuning |

> **Wire order ≠ struct order** for three fields: in memory `attributeBonus`
> (`0x18`), `quality` (`0x19`), `classification` (`0x1a`); on the wire they go
> `classification, quality, attributeBonus`. Serialize in wire order, not offset
> order.

"Compressed" = RakNet variable-length: a value drops leading zero bytes, so a
small `value`/`type` costs ~1 byte while the mostly-zero id fields
(`creatorPlayerId`, `timeout`, `stolenFromPlayerId`) cost almost nothing when 0.

## `ItemStack` (44 bytes) — an item + its instances

`ItemStack::Write` (rva `0x23cdf0`):

1. `ItemBase::Write(item)` — the shared item state (above).
2. `ids.size` — compressed 16-bit count.
3. for each id in `ids` (a `set<uint32_t>`): compressed 32-bit **instance id**.

So a stack carries the item template once, then N unique instance ids — one per
physical item in the stack. `ItemBase.value` and the id count describe the same
quantity from two angles (quantity vs. addressable instances).

## `ItemList` (36 bytes) — a container

`ItemList::Write` (rva `0x23d2c0`) emits:

1. `field@0x0` — compressed 16-bit *(unverified; capacity or container id)*
2. `field@0x14` — compressed 32-bit *(unverified)*
3. `field@0x18` — compressed 32-bit *(unverified)*
4. `field@0x1c` — compressed 32-bit *(unverified — these three may be credits /
   weight / capacity metadata)*
5. stack count — compressed 16-bit (= `(stacks.end - stacks.data) / 44`)
6. for each stack: `ItemStack::Write`

## Item / inventory packets

From `docs/packet-identifiers.md`:

| Packet | ID | Likely dir | Purpose |
| --- | --- | --- | --- |
| `ID_ITEMS_REMOVED` | 129 | S→C | items removed from a container |
| `ID_ITEMS_CHANGED` | 130 | S→C | item state changed |
| `ID_ITEM_REMOVED` | 136 | S→C | single item removed |
| `ID_MOVE_ITEMS` | 138 | C→S | move instances between containers/slots |
| `ID_MERGE_ITEMS` | 144 | C→S | merge stacks |
| `ID_ITEMS_ADDED` | 147 | S→C | items added to a container |
| `ID_TRANSFER_ITEMS` | 152 | C→S | transfer (trade/drop) |
| `ID_USE_ITEM` | 164 | C→S | use/consume |
| `ID_DEPLOY_ITEM` | 172 | C→S | deploy (placeable) |
| `ID_REPAIR_ITEM` | 173 | C→S | repair |
| `ID_RECYCLE_ITEM` | 174 | C→S | recycle |

(Directions inferred from field shape; the request ones carry `playerId` +
selection, the S→C ones carry item payloads — **directions unverified** beyond
the two decoded below.)

### `ID_MOVE_ITEMS` (138) — `Packet_ID_MOVE_ITEMS`

Body after the [[Packet Transport|VariableSizedPacket]] base (`+0x430`):

| Off | Field | Type |
| --- | --- | --- |
| `0x430` | `playerId` | u32 |
| `0x434` | `ids` | `set<u32>` (instance ids to move) |
| `0x440` | `src` | u8 (source container) |
| `0x441` | `dest` | u8 (dest container) |
| `0x442` | `srcSlot` | u8 |
| `0x443` | `destSlot` | u8 |

A move references items by **instance id** (matching the ids in `ItemStack`),
not by index — so the server resolves ids against the player's containers.

### `ID_ITEMS_ADDED` (147) — `Packet_ID_ITEMS_ADDED`

| Off | Field | Type |
| --- | --- | --- |
| `0x430` | `playerId` | u32 |
| `0x434` | `dest` | u8 (target container) |
| `0x438` | `items` | `ItemList` (the added stacks) |

The server announces additions by sending a whole `ItemList` for the target
container — i.e. the same `ItemList::Write` format above.

## Live decode

Not captured: the test character is freshly created (empty inventory) and there
is no global `Inventory` pointer symbol in the DB to anchor a live read, so a
concrete decoded stack is **unverified**. A follow-up could `xref` the inventory
packet handlers to find where the local `Inventory` is stored and read it live
after acquiring items.

## Reproduce

```bash
fomre type /FOM/Types/Item/ItemBase
fomre type /FOM/Types/Item/ItemStack
fomre type /FOM/Types/Item/ItemList
fomre type /FOM/Types/Player/Inventory
fomre type /FOM/Packets/Packet_ID_MOVE_ITEMS
fomre decompile "CShell.dll:0x10254d40"   # ItemBase::Write
fomre decompile "CShell.dll:0x1023cdf0"   # ItemStack::Write
fomre decompile "CShell.dll:0x1023d2c0"   # ItemList::Write
```

See [[Item Definitions]] (type → catalog record), [[Packet Transport]] (the
compressed-int / bit codecs), and [[WorldUpdate Wire Format]] (the same
BitStream serialization style for player state).
