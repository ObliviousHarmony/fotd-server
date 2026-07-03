# Avatar

`FOM::Types::Player::Avatar` — the player's appearance and worn-equipment block.
50 bytes, packed, all fields `u16`. The client copies it verbatim from the
engine's `PLAYERDATA_AVATAR` block into each [[WorldUpdate]] (`+0x22`).

| Offset | Field | | Offset | Field |
| --- | --- | --- | --- | --- |
| `0x00` | `sex` | | `0x10` | `slotShirt` |
| `0x02` | `skinColor` | | `0x12` | `slotBottoms` |
| `0x04` | `face` | | `0x14` | `slotShoes` |
| `0x06` | `hair` | | `0x16` | `slotHat` |
| `0x08` | `factionId` | | `0x18` | `slotHead` |
| `0x0a` | `rankId` | | `0x1a` | `slotEyes` |
| `0x0c` | *(unnamed)* | | `0x1c` | `slotShoulder` |
| `0x0e` | `legacyFaction` | | `0x1e` | `slotArms` |
| | | | `0x20` | `slotTorso` |
| | | | `0x22` | `slotBack` |
| | | | `0x24` | `slotLegs` |

The first six fields (`sex`/`skinColor`/`face`/`hair` …) are the character
creation choices — the same values stored server-side in the `player` row (see
the server vault). Reproduce: `fomre type /FOM/Types/Player/Avatar`.

## Equipment slots hold item-type ids

The `slot*` fields (`slotShirt` … `slotHands`, offsets `0x10`–`0x26`) are **item
type ids** — the same ids as [[Item Definitions]] / [[Inventory]], one per worn
piece, `0` = nothing. They are what dresses the rendered model, not the inventory
equipment container: `FOM::Game::CPlayerObj::UpdateAvatar` (`Object.lto` rva
`0x17de0`) walks the avatar's slot fields (indices 11–23 of the `u16` view) and
resolves each as an item type to pick the model/skin, then calls the dressing
helper `FUN_10006f50(avatar)`. So a peer renders your gear purely from the
`Avatar` block you broadcast in each [[WorldUpdate]] — it never sees your
inventory. Consequences for the server:

- On world entry the server must fill `RegisterClientReturn.avatar`'s slot fields
  from the player's equipped items (type id per slot) or the character spawns
  undressed even with items in the equipment container. The world server's
  `AvatarEquipment.Apply` maps each equipped `ItemSlot` → the matching `Avatar`
  field. `ItemSlot`→field: Head→`slotHead`, Eyes→`slotEyes`,
  Shoulders→`slotShoulder`, Torso→`slotTorso`, Arms→`slotArms`, Hands→`slotHands`,
  Legs→`slotLegs`, Back→`slotBack`, Hat→`slotHat`, Shirt→`slotShirt`,
  Pants→`slotBottoms`, Shoes→`slotShoes` (armour Torso/Legs and clothing
  Shirt/Pants are distinct layers with distinct fields).
- Live re-dressing is client-side: equipping in-game updates the local avatar slot
  and re-runs `UpdateAvatar`, and the new block propagates via the world update.

Reproduce: `fomre decompile Object.lto:0x10017de0`  # CPlayerObj::UpdateAvatar
