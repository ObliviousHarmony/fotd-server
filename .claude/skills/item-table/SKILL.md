---
name: item-table
description: Schema and Type->Name lookup for the global item definition array. Use when interpreting item definition records or looking up an item by type id.
---

# Item Table

Schema and lookup index for the **global item definition array** in Face of Mankind.
Per-type extracted records live in [`items/{type}.md`](./items/) as JSON.

## Where the array lives

The item definitions are static `const` records baked into each binary's **`.rdata`**.
The *same* array is compiled into multiple shells at different image bases. **Only field
offsets are stable, never addresses** — do not record absolute addresses in item files.

- **ServerShell (`Object.lto`)** — records in `.rdata`. A type->record pointer table at
  `0x101b9710` is **zero-filled in the static image and populated at load** (`table[type] = &record`);
  it is only a runtime lookup accelerator, *not* where the data lives. Example reader:
  `FUN_100c0da0(table, type)` returns `*(byte*)(record + 0x0A)`.
- **ClientShell (`CShell.dll`)** — the same records are present (asset strings duplicated).
- Game engine (`fom_client.exe`) does **not** carry these records.

Item type ids are `ushort`, valid range **1..0x0BC0 (1..3008)**. Each type maps to one 144-byte
(`0x90`) record. The records sit in `.rdata` in category-ordered blocks (not type-sorted); the
type->record association is by item type via the lookup table.

## Record schema (0x90 / 144 bytes)

Confirmed fields are named. Unproven fields keep their hex offset (`f_0xNN`) and should be
renamed **here** as they are decoded — every per-item file also carries the full `raw` bytes,
so nothing is lost.

| Offset | Type | Field | Meaning |
|-------:|------|-------|---------|
| 0x00 | u16 | `type` | item type id |
| 0x02 | u16 | `f_0x02` | subtype / sub-category (unconfirmed) |
| 0x04 | u32 | `f_0x04` | unconfirmed — **not** the gameplay Weight (that's attribute id 39 in the item attribute table); likely value/price or physical mass |
| 0x08 | u8 | `category` | **item category** (see Category table). String name = `29900 + category`. Read as a byte by `FUN_102330F0` |
| 0x09 | u8 | `subtype` | **item subtype** (see Subtype table). String name = `29931 + subtype` (except Armor, see note). Read by `FUN_102343B0` |
| 0x0A | u8 | `slot` | **`ItemSlot`** — equipment slot (see Equipment slot table); returned by `FUN_100c0da0(type)` |
| 0x0B | u8 | `f_0x0B` | unused / padding (no accessor reads it) |
| 0x0C | char* | `icon` | interface icon path (`.pcx` / `.tga`) |
| 0x10 | char* | `model` | world model path (`.ltb`) |
| 0x14 | char* | `skin` | model texture path (`.dtx`); used as avatar skin in `FUN_10006f50` |
| 0x18 | char* | `f_0x18` | alternate / active-state skin texture (`.dtx`); `""` for almost all items — only augmentation active skins set it (e.g. `scanner_on.dtx`) |
| 0x1C | char* | `f_0x1C` | base model-piece / body-region name (e.g. `Helmet7`, `Torso1`, `Legs3`); `""` for non-worn items. `FUN_10006f50` matches it against `Torso1/2`, `Legs1/3` to pick the underlying body skin |
| 0x20 | char* | `renderStyle` | render style path (`RS/…`, e.g. `RS/default.ltb`) |
| 0x24 | f32 | `f_0x24` | unconfirmed float |
| 0x28 | u32 | `f_0x28` | unconfirmed (float for some classes) |
| 0x2C | u32 | `f_0x2C` | unconfirmed |
| 0x30 | u32 | `f_0x30` | unconfirmed |
| 0x34 | u32 | `f_0x34` | unconfirmed |
| 0x38 | u8[3][8] | `flags_0x38` | **3 boolean arrays of 8** = 3 permission types × 8 factions (groups at +0x38 / +0x40 / +0x48 vary independently; all bytes 0/1). In item files: `[[8],[8],[8]]` |
| 0x50 | u32 | `f_0x50` | unconfirmed |
| 0x54 | u32 | `f_0x54` | unconfirmed |
| 0x58–0x8F | *(overlay)* | — | **category-specific overlay** — zero for non-combat items; weapon fields below. See Combat stats. |
| 0x58 | u32 | `f_0x58` | overlay flag/count (unconfirmed) |
| 0x5C | char* | `f_0x5C` | weapon **impact FX** name (e.g. `Impact_w1`); `""` otherwise |
| 0x60 | char* | `f_0x60` | weapon **muzzle FX** name (e.g. `Muzzle_w1`); `""` otherwise |
| 0x64 | u16 | `f_0x64` | weapon **ammo type** (item-type id of consumed ammo; self/0 for melee/thrown/non-weapons) |
| 0x66 | u16 | `f_0x66` | unconfirmed |
| 0x68 | u32 | `f_0x68` | unconfirmed |
| 0x6C | f32 | `f_0x6C` | weapon 0–1 ratio (accuracy/spread?) |
| 0x70 | u16 | `f_0x70` | weapon id (anim/sound/skill?) |
| 0x72 | u16 | `f_0x72` | weapon id — `93` constant across ballistics |
| 0x74 | u16 | `f_0x74` | weapon id — `94` constant across ballistics |
| 0x76 | u16 | `f_0x76` | weapon id (= `f_0x70 − 1`) |
| 0x78 | u32 | `f_0x78` | weapon small enum |
| 0x7C | u32 | `f_0x7C` | unconfirmed |
| 0x80 | u32 | `f_0x80` | unconfirmed |
| 0x84 | f32 | `f_0x84` | weapon damage-like float (descends with tier) |
| 0x88 | f32 | `f_0x88` | weapon damage-like float |
| 0x8C | f32 | `f_0x8C` | weapon damage-like float |

### Notes
- The trailing region (`0x58–0x8F`) is reused per item class. Simple items (implants, canisters,
  drugs) leave it zero; weapon/armor records pack **combat stats** there (see Combat stats below).
- `icon`, `model`, `skin`, `f_0x18`, `f_0x1C`, `renderStyle` are all `char*`. The constant
  `0x101139cc` is the shared **empty-string sentinel** (`""`) — in extracted files it is resolved
  to `""`. (It sits a few bytes before an unrelated `WorldSection` vftable; it is not an object.)
- Remaining `f_0xNN` fields are not yet proven — treat as raw until confirmed.

## Item category (`+0x08`)

Single byte. Names are the game's own UI strings (`CRes.dll` string `29900 + category`),
not guesses. Exposed in each item file as `category` / `category_name`.

| Cat | Name | Cat | Name |
|----:|------|----:|------|
| 0x01 | Medication | 0x0D | Premium Torso Clothes |
| 0x02 | Ammunition | 0x0E | Premium Leg Clothes |
| 0x03 | Weapon | 0x0F | Shoes |
| 0x04 | Explosive | 0x10 | Raw Material |
| 0x05 | Armor | 0x11 | Production Element |
| 0x06 | Mechanical Augmentation | 0x12 | Loot |
| 0x07 | Glasses | 0x13 | Production Tool |
| 0x08 | Food | 0x14 | Nano-Augmentation |
| 0x09 | Booster | 0x15 | Deployable |
| 0x0A | Miscellaneous | 0x16 | Territory Deployable |
| 0x0B | Torso Clothes | 0x17 | Hat |
| 0x0C | Leg Clothes | 0x18 | Marketplace |

(`category 0` = "- All Item Types -", a filter sentinel, never a real item.)

## Item subtype (`+0x09`)

Single byte, a **global** enum (game string `29931 + subtype`). Meaning is shared across the item
space — weapons use the weapon-class values, materials use the material values, etc.
**Exception: for Armor (category 5)** the displayed subtype is computed specially
(`FUN_10232750`/`FUN_10232C30`) as an armor brand/tier, so `29931 + subtype` does **not** apply.

| Sub | Name | Sub | Name | Sub | Name |
|----:|------|----:|------|----:|------|
| 0 | - All Subtypes - | 13 | Ballistic Precision Rifle | 26 | Carbon Material |
| 1 | Medikit | 14 | Grenade | 27 | Metal Component |
| 2 | Injector | 15 | Turret | 28 | Glass Material |
| 3 | Light Ballistic | 16 | Energy Handgun | 29 | Plastic Material |
| 4 | Heavy Ballistic | 17 | Energy Rifle | 30 | Syntactic Foam |
| 5 | Light Energy | 18 | Medical | 31 | Electronic |
| 6 | Heavy Energy | 19 | Tazer | 32 | Advanced Electronic |
| 7 | Hacking | 20 | Energy Anti-Alien | 33 | Carbon Component |
| 8 | Influence Generator | 21 | Energy Precision Rifle | 34 | Chemical Component |
| 9 | Ballistic Handgun | 22 | Mining Tool | 35 | Alien Component |
| 10 | Ballistic Knive | 23 | Ballistic | 36 | Datacube |
| 11 | Ballistic Rifle | 24 | Energy | 37 | Standard |
| 12 | Ballistic SMG | 25 | Mineral Collector | 38 | Ballistic Anti-Alien |

## Equipment slot (`+0x0A`) — `ItemSlot`

Single byte = the `ItemSlot` the item equips into (returned by `FUN_100c0da0(type)`).
Augmentations map to their body location, not a generic "aug" slot.

| Value | ItemSlot | Value | ItemSlot |
|------:|----------|------:|----------|
| 0 | `ITEM_SLOT_INVENTORY` (consumables/ammo/materials/loot) | 11 | `ITEM_SLOT_LEGS` |
| 1 | `ITEM_SLOT_WEAPON` | 12 | `ITEM_SLOT_BACK` (Shield aug) |
| 2–4 | *(reserved/unused)* | 13 | `ITEM_SLOT_HAT` |
| 5 | `ITEM_SLOT_HEAD` | 14 | `ITEM_SLOT_SHIRT` |
| 6 | `ITEM_SLOT_EYES` | 15 | `ITEM_SLOT_PANTS` |
| 7 | `ITEM_SLOT_SHOULDERS` | 16 | `ITEM_SLOT_SHOES` |
| 8 | `ITEM_SLOT_TORSO` | 18–21 | Quickslot *(runtime only — no item declares it)* |
| 9 | `ITEM_SLOT_ARMS` | 26 | `ITEM_SLOT_NANO_AUG` |
| 10 | `ITEM_SLOT_HANDS` | | |

(`ITEM_SLOT_MURDER_CARD` = `52`, used only by type 980 "Murder ID Card" — a special slot for the
murder-registration interface, not a body/equipment slot. The feature may no longer be in the game.)

## Combat stats & attributes

Item stats are **per-item attribute deltas** summed into the player's 53-entry attribute vector on
equip. They are surfaced in each item file's **`# Attributes`** block (attribute name → value);
**188** item types have stats.

- **Static source (`.rdata`): `DAT_10143388`** — a const table of **411 rows × 8 bytes**, each
  `{u16 item_type, u16 attribute_id, u32 value}`. Authoritative item-stat data.
- **Runtime:** lazily loaded by `FUN_100D0420` (guard `DAT_101BD050`) into `DAT_101BD058[type]` —
  one `std::vector` (16-byte descriptor) of `{attribute_id, value}` pairs per item type (1..0xBC0).
- **Application:** `FUN_100E3500` (per tick) walks equipped slots and sums each item's deltas into
  the player `maxValues[id]` (`PlayerAttributes + 0x1A8 + id*4`), clamped by cap table `DAT_101431A0`.
- **`PlayerAttributes`** (`/FOM/Types/Player/`, 944 bytes: `current[53]`/`default[53]`/`max[53]`/
  `dirtyFlags[53]`/`old[53]`, u32; defaults `DAT_10143278`) is **network-replicated** — `Read`/`Write`
  (@0x100E34A0 / 0x100E3440) do 53x `BitStream::ReadCompressed(...,0x20)`.

The record overlay (`0x58-0x8F`) is **separate** from this system: it carries the weapon ammo type
(`f_0x64`) and FX names (`f_0x5C`/`f_0x60`); its damage-like floats (`f_0x84/88/8C`) are read by **no
client code** and are superseded by the attribute table (overlay `63.0` vs the attribute's Ballistic
Damage `64`) — likely legacy/display.

### Attribute reference

`key` = the camelCase key used in `attributes/{type}.json`. `id` = attribute index (label = game
string `6300 + id`; player slot at `PlayerAttributes + 0x1A8 + id*4`). `default` / `max` are the
player base/cap from `DAT_10143278` / `DAT_101431A0` (`0xFFFFFFFF` = uncapped).

| id | key | label | default | max |
|---:|-----|-------|--------:|----:|
| 0 | `health` | Health | 1000 | 1000 |
| 1 | `stamina` | Stamina | 0 | 10000 |
| 2 | `bioEnergy` | Bio Energy | 0 | 1000 |
| 3 | `aura` | Aura | 1000 | 1000 |
| 4 | `universalCredits` | Universal Credits | 0 | 1000000000 |
| 5 | `factionCredits` | Faction Credits | 0 | 1000000000 |
| 6 | `penalty` | Penalty | 0 | 1000000000 |
| 7 | `prisonerStatus` | Prisoner Status | 0 | 32 |
| 8 | `highestPenalty` | Highest Penalty | 0 | 1000000000 |
| 9 | `mostWantedStatus` | Most-Wanted Status | 0 | 1 |
| 10 | `wantedStatus` | Wanted Status | 0 | 1 |
| 11 | `agility` | Agility | 900 | 1000 |
| 12 | `ballisticDamage` | Ballistic Damage | 0 | 1000 |
| 13 | `energyDamage` | Energy Damage | 0 | 1000 |
| 14 | `bioDamage` | Bio Damage | 0 | 1000 |
| 15 | `auraDamage` | Aura Damage | 0 | 1000 |
| 16 | `destruction` | Destruction | 0 | 10000 |
| 17 | `weaponRecoil` | Weapon Recoil | 0 | 3000 |
| 18 | `armor` | Armor | 0 | 1000 |
| 19 | `shielding` | Shielding | 0 | 1000 |
| 20 | `resistance` | Resistance | 0 | 1000 |
| 21 | `reflection` | Reflection | 0 | 1000 |
| 22 | `healthRegeneration` | Health Regeneration | 0 | 1000 |
| 23 | `staminaRegeneration` | Stamina Regeneration | 0 | 600 |
| 24 | `bioRegeneration` | Bio Regeneration | 0 | 1000 |
| 25 | `auraRegeneration` | Aura Regeneration | 30 | 1000 |
| 26 | `coins` | Coins | 0 | 1000000000 |
| 27 | `healingCooldown` | Healing Cooldown | 0 | 0xFFFFFFFF |
| 28 | `foodCooldown` | Food Cooldown | 0 | 0xFFFFFFFF |
| 29 | `xenoDamage` | Xeno Damage | 0 | 1000 |
| 30 | `healthDrain` | Health Drain | 0 | 1000 |
| 31 | `staminaDrain` | Stamina Drain | 0 | 1000 |
| 32 | `bioEnergyDrain` | Bio Energy Drain | 0 | 1000 |
| 33 | `auraDrain` | Aura Drain | 0 | 1000 |
| 34 | `protectionBypass` | Protection Bypass | 0 | 1000 |
| 35 | `effectiveRange` | Effective Range | 0 | 5000 |
| 36 | `weaponFireDelay` | Weapon Fire Delay | 0 | 2000 |
| 37 | `blank1` | Blank 1 | 0 | 1000 |
| 38 | `blank2` | Blank 2 | 0 | 1000 |
| 39 | `weight` | Weight | 0 | 500000 |
| 40 | `jumpVelocityMultiplier` | Jump Velocity Multiplier | 1000 | 2000 |
| 41 | `fallDamageMultiplier` | Fall Damage Multiplier | 1000 | 1000 |
| 42 | `nightvision` | Nightvision | 0 | 1 |
| 43 | `soundlessMovement` | Soundless Movement | 0 | 1 |
| 44 | `activationDistance` | Activation Distance | 125 | 500 |
| 45 | `sprintSpeedMultiplier` | Sprint Speed Multiplier | 1000 | 2000 |
| 46 | `maxStamina` | Max Stamina | 4000 | 10000 |
| 47 | `bioEnergyReplenishingCooldown` | Bio Energy Replenishing Cooldown | 0 | 0xFFFFFFFF |
| 48 | `auraHealingCooldown` | Aura Healing Cooldown | 0 | 0xFFFFFFFF |
| 49 | `emergencyShield` | Emergency Shield | 0 | 5 |
| 50 | `emergencyShieldCooldown` | Emergency Shield Cooldown | 0 | 300 |
| 51 | `vortexEmitterCountdown` | Vortex Emitter Countdown | 0 | 30 |
| 52 | `attr52` | *(no string 6352)* | 0 | 600 |

Item files carry only the attributes an item actually modifies (sparse). Armor protection values
are also delivered through this attribute table (id 18 Armor, 19 Shielding, 20 Resistance, …).

## Coverage

- **1075 / 1109** item types from `docs/items.md` have a distinct record.
- **34** types have **no distinct record** ("alias" below) — they reuse another item's
  definition (mostly civilian clothing variants, plus Skill Book and the Lockouts).

## Per-item files

Straight JSON, one file per item type, address-free (identical data across shells):
- **`definitions/{type}.json`** — flat object of the decoded record fields (offset-keyed `f_0xNN`
  plus the named `type`/`category`/`subtype`/`slot`/`icon`/`model`/`skin`/`renderStyle`/`flags_0x38`).
  Field → offset mapping is the schema table above. No `name`/`category_name` (derive from the
  Type→Name and Category tables), no `raw` bytes, no addresses. **All 1075** items.
- **`attributes/{type}.json`** — flat object of `attributeKey → value` for the stats the item
  grants (from `DAT_10143388`). Only the **188** items with stats have a file (absent = no stats).
  `attributeKey` is the camelCase key in the Attribute key table below.

### Locating a record in Ghidra
Addresses differ per shell, so they aren't stored. To find a record's base in either shell:
xref the item's `icon` string (`Interface/Items/…`) — it has a single DATA ref from the record's
icon field — and subtract `0x0C`. (Or `model` − `0x10`, `skin` − `0x14`.)

## Item Type -> Name

`rec` = has a distinct record; `alias` = no distinct record (reuses another definition).

| Type | Name | |
|-----:|------|--|

| 1 | Zanathid 5 Inflex | rec |
| 2 | Ryx 7 Streamline | rec |
| 3 | RGI-9 | rec |
| 4 | Orbit XB Infineon | rec |
| 5 | Armalite Survival | rec |
| 6 | Enfield Life Protector | rec |
| 7 | FGz-78 | rec |
| 8 | Militek EMP | rec |
| 9 | Techtronic 6x6 | rec |
| 10 | HR420 | rec |
| 11 | Linner PP7 | rec |
| 12 | DOA 187 | rec |
| 13 | Salvotec HP220 | rec |
| 14 | EMP-Rv3 | rec |
| 15 | Gakk MG6 | rec |
| 16 | Hallem TAR7 Tactical Assault Rifle | rec |
| 17 | Protonix Barracuda | rec |
| 18 | Chrono Enervon Pistol EP24 | rec |
| 19 | Aurelian Technologies Dominator | rec |
| 20 | Enfield Incapacitator Rifle ER-X | rec |
| 21 | CryoTech Medigun CM2 | rec |
| 22 | Personal Turret | rec |
| 23 | Experimental X-01 Gun | rec |
| 24 | RK 2a7 Mining Tool | rec |
| 25 | RX Zephyr Advanced Mining Tool | rec |
| 26 | Cucurbita Grenade | rec |
| 27 | Arachnid Explosive | rec |
| 28 | Hallem IX-78 Precision Energy Rifle | rec |
| 29 | CryoTech Medigun CM1 | rec |
| 30 | Frostbite | rec |
| 31 | U-V890 Hyper Advanced Mining Tool | rec |
| 32 | Linner PP-X | rec |
| 33 | Hawk-72 | rec |
| 34 | Zanathid 5.5 CopKiller | rec |
| 35 | Backer Rifle | rec |
| 36 | Territory Turret | rec |
| 37 | Golden Zanathid 5 Inflex | rec |
| 50 | 9mm Standard Rounds | rec |
| 51 | Energy Cell | rec |
| 52 | 7.62mm Standard Rounds | rec |
| 53 | High Capacity Energy Cell | rec |
| 54 | Plutonium Cell | rec |
| 55 | 5.54mm Standard Rounds | rec |
| 56 | 7.62mm Titanium Rounds | rec |
| 57 | 9mm Rubber Rounds | rec |
| 58 | 7.62mm Rubber Rounds | rec |
| 59 | 7.62mm Electromagnetic Rounds | rec |
| 80 | Biocell | rec |
| 81 | Small MediKit | rec |
| 82 | Standard MediKit | rec |
| 83 | Battle MediKit | rec |
| 84 | Emergency MediKit | rec |
| 85 | Nanomachine Autoinjector | rec |
| 86 | Nanomachine Autoinjector Mk2 | rec |
| 87 | Adrenaline Autoinjector | rec |
| 88 | Electrolyte Autoinjector | rec |
| 100 | Night Vision Augmentation | rec |
| 101 | Stamina Amplification | rec |
| 102 | Shoulder Lamp | rec |
| 103 | Resistance Amp | rec |
| 104 | Shield Augmentation | rec |
| 105 | Infiltration Scanner Augmentation | rec |
| 106 | Portable Vortex Particle Emitter | rec |
| 107 | Mineral Scanner | rec |
| 108 | Hunter VX270 | rec |
| 110 | Advanced Civilian Helmet | rec |
| 111 | MT-27 Ballistics Helmet | rec |
| 112 | Tactical Systems Helmet | rec |
| 113 | Locans Defense Helmet | rec |
| 114 | Locans Ethereal Helmet | rec |
| 115 | Locans Patrol Helmet | rec |
| 116 | Locans Pressured Helmet | rec |
| 117 | Pythica Special Operations Helmet | rec |
| 118 | Pythica Mobile Infantry Helmet | rec |
| 119 | Pythica S1 Helmet | rec |
| 120 | Pythica Sustained Battle Helmet | rec |
| 121 | NanoTech Trauma Helmet | rec |
| 122 | NanoTech Scout Helmet | rec |
| 123 | NanoTech Voltaic Helmet | rec |
| 124 | NanoTech Cognizant Helmet | rec |
| 125 | Hypobaric Helmet | rec |
| 126 | Metabolic Helmet | rec |
| 127 | Venom Helmet | rec |
| 128 | Detox Combat Helmet | rec |
| 129 | Delta Powered Helmet | rec |
| 130 | Havoc Powered Helmet | rec |
| 131 | Firstborn Powered Helmet | rec |
| 132 | Legionnaire Powered Helmet | rec |
| 133 | PreMet Buffer Helmet | rec |
| 134 | PreMet Contact Helmet | rec |
| 135 | PreMet Collision Helmet | rec |
| 136 | PreMet Impact Helmet | rec |
| 137 | Dilatant 46b Helmet | rec |
| 138 | Aramid Modified Helmet | rec |
| 139 | Dilatant 50b Helmet | rec |
| 140 | Aramid Basic Helmet | rec |
| 141 | Infensus Minimalist Helmet | rec |
| 142 | Infensus Shock Helmet | rec |
| 143 | Infensus X1 Assault Helmet | rec |
| 144 | Infensus Heavy Helmet | rec |
| 145 | Locans Patrol Helmet | rec |
| 146 | Pythica S2 Helmet | rec |
| 147 | NanoTech Voltaic Assault Helmet | rec |
| 148 | Venom Helmet | rec |
| 149 | Firstborn Powered Helmet | rec |
| 150 | PreMet Collision Helmet | rec |
| 151 | Dilatant 50b Helmet | rec |
| 152 | Infensus X2 Assault Helmet | rec |
| 153 | XenoTech Expeditionary Helmet | rec |
| 154 | Exile Helmet | rec |
| 155 | Black Shuck Helmet | rec |
| 156 | Echion Helmet | rec |
| 157 | Locans Stabilized Helmet | rec |
| 158 | Pythica Durable Battle Helmet | rec |
| 159 | NanoTech Vitality Helmet | rec |
| 160 | Lintems Glasses | rec |
| 161 | DesIns Glasses | rec |
| 162 | Imaxil Incorporated Glasses | rec |
| 163 | ALM designs Glasses (Blue) | rec |
| 164 | Nology Glasses (Blue) | rec |
| 165 | Zivce6 Glasses (Blue) | rec |
| 166 | Mica\ | rec |
| 167 | ALM Designs Glasses (Black) | rec |
| 168 | Urban Dreams Glasses | rec |
| 169 | Zivce6 Glasses (Grey) | rec |
| 170 | Mica\ | rec |
| 171 | ALM Designs Glasses (Green) | rec |
| 172 | Nology Glasses (Yellow) | rec |
| 173 | Zivce6 Glasses (Yellow) | rec |
| 174 | Mica\ | rec |
| 175 | ALM Designs Glasses (Red) | rec |
| 176 | Nology Glasses (Red) | rec |
| 177 | Zivce6 Glasses (Red) | rec |
| 178 | Mica\ | rec |
| 179 | ALM Designs Glasses (Grey) | rec |
| 180 | Nology Glasses (Grey) | rec |
| 181 | Zivce6 Glasses (Olive Green) | rec |
| 182 | Mica\ | rec |
| 183 | ALM Designs Glasses (Yellow) | rec |
| 184 | Nology Glasses (Brown) | rec |
| 185 | Zivce6 Glasses (Yellow) | rec |
| 186 | Mica\ | rec |
| 187 | ALM Designs Glasses (Silver) | rec |
| 188 | Nology Glasses (Silver) | rec |
| 189 | Zivce6 Glasses (Silver) | rec |
| 190 | Mica\ | rec |
| 191 | ALM Designs Glasses (Green) | rec |
| 192 | Nology Glasses (Green) | rec |
| 193 | Zivce6 Glasses (Green) | rec |
| 194 | Mica\ | rec |
| 195 | GIS Shades | rec |
| 196 | Fire Orange Glasses | rec |
| 197 | Incognito Shades | rec |
| 200 | Leech Helmet | rec |
| 201 | Justicar Powered Helmet | rec |
| 202 | PreMet Tremor Helmet | rec |
| 203 | Aramid Altered Helmet | rec |
| 204 | Infensus Essentials Helmet | rec |
| 205 | Jack-o'-lantern Helmet | rec |
| 206 | Skeleton Helmet | rec |
| 207 | Backer Helmet | rec |
| 210 | Advanced Civilian Shoulder Pads | rec |
| 211 | MT-27 Ballistics Shoulder Pads | rec |
| 212 | Tactical Systems Shoulder Pads | rec |
| 213 | Locans Defense Shoulder Pads | rec |
| 214 | Locans Ethereal Shoulder Pads | rec |
| 215 | Locans Patrol Shoulder Pads | rec |
| 216 | Locans Pressured Shoulder Pads | rec |
| 217 | Pythica Special Operations Shoulder Pads | rec |
| 218 | Pythica Mobile Infantry Shoulder Pads | rec |
| 219 | Pythica S1 Shoulder Pads | rec |
| 220 | Pythica Sustained Battle Shoulder Pads | rec |
| 221 | NanoTech Trauma Shoulder Pads | rec |
| 222 | NanoTech Scout Shoulder Pads | rec |
| 223 | NanoTech Voltaic Shoulder Pads | rec |
| 224 | NanoTech Cognizant Shoulder Pads | rec |
| 225 | Hypobaric Shoulder Pads | rec |
| 226 | Metabolic Shoulder Pads | rec |
| 227 | Venom Shoulder Pads | rec |
| 228 | Detox Combat Shoulder Pads | rec |
| 229 | Delta Powered Shoulder Pads | rec |
| 230 | Havoc Powered Shoulder Pads | rec |
| 231 | Firstborn Powered Shoulder Pads | rec |
| 232 | Legionnaire Powered Shoulder Pads | rec |
| 233 | PreMet Buffer Shoulder Pads | rec |
| 234 | PreMet Contact Shoulder Pads | rec |
| 235 | PreMet Collision Shoulder Pads | rec |
| 236 | PreMet Impact Shoulder Pads | rec |
| 237 | Dilatant 46b Shoulder Pads | rec |
| 238 | Aramid Modified Shoulder Pads | rec |
| 239 | Dilatant 50b Shoulder Pads | rec |
| 240 | Aramid Basic Shoulder Pads | rec |
| 241 | Infensus Minimalist Shoulder Pads | rec |
| 242 | Infensus Shock Shoulder Pads | rec |
| 243 | Infensus X1 Assault Shoulder Pads | rec |
| 244 | Infensus Heavy Shoulder Pads | rec |
| 245 | XenoTech Expeditionary Shoulder Pads | rec |
| 246 | Exile Shoulder Pads | rec |
| 247 | Locans Stabilized Shoulder Pads | rec |
| 248 | Pythica Durable Battle Shoulder Pads | rec |
| 249 | NanoTech Vitality Shoulder Pads | rec |
| 250 | Leech Shoulder Pads | rec |
| 251 | Justicar Powered Shoulder Pads | rec |
| 252 | PreMet Tremor Shoulder Pads | rec |
| 253 | Aramid Altered Shoulder Pads | rec |
| 254 | Infensus Essentials Shoulder Pads | rec |
| 255 | Jack-o'-lantern Shoulder Pads | rec |
| 256 | Skeleton Shoulder Pads | rec |
| 257 | Backer Shoulder Pads | rec |
| 260 | Advanced Civilian Arm Pads | rec |
| 261 | MT-27 Ballistics Arm Pads | rec |
| 262 | Tactical Systems Arm Pads | rec |
| 263 | Locans Defense Arm Pads | rec |
| 264 | Locans Ethereal Arm Pads | rec |
| 265 | Locans Patrol Arm Pads | rec |
| 266 | Locans Pressured Arm Pads | rec |
| 267 | Pythica Special Operations Arm Pads | rec |
| 268 | Pythica Mobile Infantry Arm Pads | rec |
| 269 | Pythica S1 Arm Pads | rec |
| 270 | Pythica Sustained Battle Arm Pads | rec |
| 271 | NanoTech Trauma Arm Pads | rec |
| 272 | NanoTech Scout Arm Pads | rec |
| 273 | NanoTech Voltaic Arm Pads | rec |
| 274 | NanoTech Cognizant Arm Pads | rec |
| 275 | Hypobaric Arm Pads | rec |
| 276 | Metabolic Arm Pads | rec |
| 277 | Venom Arm Pads | rec |
| 278 | Detox Combat Arm Pads | rec |
| 279 | Delta Powered Arm Pads | rec |
| 280 | Havoc Powered Arm Pads | rec |
| 281 | Firstborn Powered Arm Pads | rec |
| 282 | Legionnaire Powered Arm Pads | rec |
| 283 | PreMet Buffer Arm Pads | rec |
| 284 | PreMet Contact Arm Pads | rec |
| 285 | PreMet Collision Arm Pads | rec |
| 286 | PreMet Impact Arm Pads | rec |
| 287 | Dilatant 46b Arm Pads | rec |
| 288 | Aramid Modified Arm Pads | rec |
| 289 | Dilatant 50b Arm Pads | rec |
| 290 | Aramid Basic Arm Pads | rec |
| 291 | Infensus Minimalist Arm Pads | rec |
| 292 | Infensus Shock Arm Pads | rec |
| 293 | Infensus X1 Assault Arm Pads | rec |
| 294 | Infensus Heavy Arm Pads | rec |
| 295 | XenoTech Expeditionary Arm Pads | rec |
| 296 | Exile Arm Pads | rec |
| 297 | Locans Stabilized Arm Pads | rec |
| 298 | Pythica Durable Battle Arm Pads | rec |
| 299 | NanoTech Vitality Arm Pads | rec |
| 300 | Leech Arm Pads | rec |
| 301 | Justicar Powered Arm Pads | rec |
| 302 | PreMet Tremor Arm Pads | rec |
| 303 | Aramid Altered Arm Pads | rec |
| 304 | Infensus Essentials Arm Pads | rec |
| 305 | Jack-o'-lantern Arm Pads | rec |
| 306 | Skeleton Arm Pads | rec |
| 307 | Backer Arm Pads | rec |
| 310 | Advanced Civilian Torso Armor | rec |
| 311 | MT-27 Ballistics Torso Armor | rec |
| 312 | Tactical Systems Torso Armor | rec |
| 313 | Locans Defense Torso Armor | rec |
| 314 | Locans Ethereal Torso Armor | rec |
| 315 | Locans Patrol Torso Armor | rec |
| 316 | Locans Pressured Torso Armor | rec |
| 317 | Pythica Special Operations Torso Armor | rec |
| 318 | Pythica Mobile Infantry Torso Armor | rec |
| 319 | Pythica S1 Torso Armor | rec |
| 320 | Pythica Sustained Battle Torso Armor | rec |
| 321 | NanoTech Trauma Torso Armor | rec |
| 322 | NanoTech Scout Torso Armor | rec |
| 323 | NanoTech Voltaic Torso Armor | rec |
| 324 | NanoTech Cognizant Torso Armor | rec |
| 325 | Hypobaric Torso Armor | rec |
| 326 | Metabolic Torso Armor | rec |
| 327 | Venom Torso Armor | rec |
| 328 | Detox Combat Torso Armor | rec |
| 329 | Delta Powered Torso Armor | rec |
| 330 | Havoc Powered Torso Armor | rec |
| 331 | Firstborn Powered Torso Armor | rec |
| 332 | Legionnaire Powered Torso Armor | rec |
| 333 | PreMet Buffer Torso Armor | rec |
| 334 | PreMet Contact Torso Armor | rec |
| 335 | PreMet Collision Torso Armor | rec |
| 336 | PreMet Impact Torso Armor | rec |
| 337 | Dilatant 46b Torso Armor | rec |
| 338 | Aramid Modified Torso Armor | rec |
| 339 | Dilatant 50b Torso Armor | rec |
| 340 | Aramid Basic Torso Armor | rec |
| 341 | Infensus Minimalist Torso Armor | rec |
| 342 | Infensus Shock Torso Armor | rec |
| 343 | Infensus X1 Assault Torso Armor | rec |
| 344 | Infensus Heavy Torso Armor | rec |
| 345 | XenoTech Expeditionary Torso Armor | rec |
| 346 | Exile Torso Armor | rec |
| 347 | Locans Stabilized Torso Armor | rec |
| 348 | Pythica Durable Battle Torso Armor | rec |
| 349 | NanoTech Vitality Torso Armor | rec |
| 350 | Leech Torso Armor | rec |
| 351 | Justicar Powered Torso Armor | rec |
| 352 | PreMet Tremor Torso Armor | rec |
| 353 | Aramid Altered Torso Armor | rec |
| 354 | Infensus Essentials Torso Armor | rec |
| 355 | Jack-o'-lantern Torso Armor | rec |
| 356 | Skeleton Torso Armor | rec |
| 357 | Backer Torso Armor | rec |
| 360 | Advanced Civilian Leg Pads | rec |
| 361 | MT-27 Ballistics Leg Pads | rec |
| 362 | Tactical Systems Leg Pads | rec |
| 363 | Locans Defense Leg Pads | rec |
| 364 | Locans Ethereal Leg Pads | rec |
| 365 | Locans Patrol Leg Pads | rec |
| 366 | Locans Pressured Leg Pads | rec |
| 367 | Pythica Special Operations Leg Pads | rec |
| 368 | Pythica Mobile Infantry Leg Pads | rec |
| 369 | Pythica S1 Leg Pads | rec |
| 370 | Pythica Sustained Battle Leg Pads | rec |
| 371 | NanoTech Trauma Leg Pads | rec |
| 372 | NanoTech Scout Leg Pads | rec |
| 373 | NanoTech Voltaic Leg Pads | rec |
| 374 | NanoTech Cognizant Leg Pads | rec |
| 375 | Hypobaric Leg Pads | rec |
| 376 | Metabolic Leg Pads | rec |
| 377 | Venom Leg Pads | rec |
| 378 | Detox Combat Leg Pads | rec |
| 379 | Delta Powered Leg Pads | rec |
| 380 | Havoc Powered Leg Pads | rec |
| 381 | Firstborn Powered Leg Pads | rec |
| 382 | Legionnaire Powered Leg Pads | rec |
| 383 | PreMet Buffer Leg Pads | rec |
| 384 | PreMet Contact Leg Pads | rec |
| 385 | PreMet Collision Leg Pads | rec |
| 386 | PreMet Impact Leg Pads | rec |
| 387 | Dilatant 46b Leg Pads | rec |
| 388 | Aramid Modified Leg Pads | rec |
| 389 | Dilatant 50b Assault Leg Pads | rec |
| 390 | Aramid Basic Leg Pads | rec |
| 391 | Infensus Minimalist Leg Pads | rec |
| 392 | Infensus Shock Leg Pads | rec |
| 393 | Infensus X1 Assault Leg Pads | rec |
| 394 | Infensus Heavy Leg Pads | rec |
| 395 | XenoTech Expeditionary Leg Pads | rec |
| 396 | Exile Leg Pads | rec |
| 397 | Locans Stabilized Leg Pads | rec |
| 398 | Pythica Durable Battle Leg Pads | rec |
| 399 | NanoTech Vitality Leg Pads | rec |
| 400 | Leech Leg Pads | rec |
| 401 | Justicar Powered Leg Pads | rec |
| 402 | PreMet Tremor Leg Pads | rec |
| 403 | Aramid Altered Leg Pads | rec |
| 404 | Infensus Essentials Leg Pads | rec |
| 405 | Jack-o'-lantern Leg Pads | rec |
| 406 | Skeleton Leg Pads | rec |
| 407 | Backer Leg Pads | rec |
| 410 | Envirotech Gloves (Male) | rec |
| 411 | Infusion Gloves (Male) | rec |
| 412 | Infostyle Gloves (Male) | rec |
| 413 | Locans Defense Gloves (Male) | rec |
| 414 | Locans Ethereal Gloves (Male) | rec |
| 415 | Locans Patrol Gloves (Male) | rec |
| 416 | Locans Pressured Gloves (Male) | rec |
| 417 | Pythica Special Operations Gloves (Male) | rec |
| 418 | Pythica Mobile Infantry Gloves (Male) | rec |
| 419 | Pythica S1 Gloves (Male) | rec |
| 420 | Pythica Sustained Battle Gloves (Male) | rec |
| 421 | NanoTech Trauma Gloves (Male) | rec |
| 422 | NanoTech Scout Gloves (Male) | rec |
| 423 | NanoTech Voltaic Gloves (Male) | rec |
| 424 | NanoTech Cognizant Gloves (Male) | rec |
| 425 | Hypobaric Gloves (Male) | rec |
| 426 | Metabolic Gloves (Male) | rec |
| 427 | Venom Gloves (Male) | rec |
| 428 | Detox Combat Gloves (Male) | rec |
| 429 | Delta Powered Gloves (Male) | rec |
| 430 | Havoc Powered Gloves (Male) | rec |
| 431 | Firstborn Powered Gloves (Male) | rec |
| 432 | Legionnaire Powered Gloves (Male) | rec |
| 433 | PreMet Buffer Gloves (Male) | rec |
| 434 | PreMet Contact Gloves (Male) | rec |
| 435 | PreMet Collision Gloves (Male) | rec |
| 436 | PreMet Impact Gloves (Male) | rec |
| 437 | Dilatant 46b Gloves (Male) | rec |
| 438 | Aramid Modified Gloves (Male) | rec |
| 439 | Dilatant 50b Gloves (Male) | rec |
| 440 | Aramid Basic Gloves (Male) | rec |
| 441 | Infensus Minimalist Gloves (Male) | rec |
| 442 | Infensus Shock Gloves (Male) | rec |
| 443 | Infensus X1 Assault Gloves (Male) | rec |
| 444 | Infensus Heavy Gloves (Male) | rec |
| 445 | Hockey Gloves (Female) | rec |
| 446 | Egyptian Knight (Female) | rec |
| 447 | Crystal Gloves (Female) | rec |
| 448 | Locans Defense Gloves(Female) | rec |
| 449 | Locans Ethereal Gloves (Female) | rec |
| 450 | Locans Patrol Gloves (Female) | rec |
| 451 | Locans Pressured Gloves (Female) | rec |
| 452 | Pythica Special Operations Gloves (Female) | rec |
| 453 | Pythica Mobile Infantry Gloves (Female) | rec |
| 454 | Pythica S1 Gloves (Female) | rec |
| 455 | Pythica Sustained Battle Gloves (Female) | rec |
| 456 | NanoTech Trauma Gloves (Female) | rec |
| 457 | NanoTech Scout Gloves (Female) | rec |
| 458 | NanoTech Voltaic Gloves (Female) | rec |
| 459 | NanoTech Cognizant Gloves (Female) | rec |
| 460 | Hypobaric Gloves (Female) | rec |
| 461 | Metabolic Gloves (Female) | rec |
| 462 | Venom Gloves (Female) | rec |
| 463 | Detox Combat Gloves (Female) | rec |
| 464 | Delta Powered Gloves (Female) | rec |
| 465 | Havoc Powered Gloves (Female) | rec |
| 466 | Firstborn Powered Gloves (Female) | rec |
| 467 | Legionnaire Powered Gloves (Female) | rec |
| 468 | PreMet Buffer Gloves (Female) | rec |
| 469 | PreMet Contact Gloves (Female) | rec |
| 470 | PreMet Collision Gloves (Female) | rec |
| 471 | PreMet Impact Gloves (Female) | rec |
| 472 | Dilatant 46b Gloves (Female) | rec |
| 473 | Aramid Modified Gloves (Female) | rec |
| 474 | Dilatant 50b Gloves (Female) | rec |
| 475 | Aramid Basic Gloves (Female) | rec |
| 476 | Infensus Minimalist Gloves (Female) | rec |
| 477 | Infensus Shock Gloves (Female) | rec |
| 478 | Infensus X1 Assault Gloves (Female) | rec |
| 479 | Infensus Heavy Gloves (Female) | rec |
| 480 | Incognito Gloves (Male) | rec |
| 481 | XenoTech Expeditionary Gloves (Male) | rec |
| 482 | Incognito Gloves (Female) | rec |
| 483 | Skeleton Gloves (Male) | rec |
| 484 | Skeleton Gloves (Female) | rec |
| 485 | Jack-o'-lantern Gloves (Male) | rec |
| 486 | Jack-o'-lantern Gloves (Female) | rec |
| 487 | Locans Stabilized Gloves (Female) | rec |
| 488 | Locans Stabilized Gloves (Male) | rec |
| 489 | Pythica Durable Battle Gloves (Female) | rec |
| 490 | Pythica Durable Battle Gloves (Male) | rec |
| 491 | NanoTech Vitality Gloves (Female) | rec |
| 492 | NanoTech Vitality Gloves (Male) | rec |
| 493 | Leech Gloves (Female) | rec |
| 494 | Leech Gloves (Male) | rec |
| 495 | Justicar Powered Gloves (Female) | rec |
| 496 | Justicar Powered Gloves (Male) | rec |
| 497 | PreMet Tremor Gloves (Female) | rec |
| 498 | PreMet Tremor Gloves (Male) | rec |
| 499 | Aramid Altered Gloves (Female) | rec |
| 500 | Black Dress Shoes (Male) | rec |
| 501 | Brown Dress Shoes (Male) | rec |
| 502 | White Dress Shoes (Male) | rec |
| 503 | Indirect Design Shoes (Male) | rec |
| 504 | Silent Whisper Shoes (Male) | rec |
| 505 | Fear To Tread Shoes (Male) | rec |
| 506 | Perfect Step Shoes (Male) | rec |
| 507 | Milatech Shoes (Male) | rec |
| 508 | Discreet Dress Shoes (Male) | rec |
| 509 | Fine Line Shoes (Male) | rec |
| 510 | Lizard Tech Blue Shoes(Female) | rec |
| 511 | Lizard Tech Black Shoes (Female) | rec |
| 512 | Scarpa Runner Shoes (Female) | rec |
| 513 | Scarpa Solid Shoes (Female) | rec |
| 514 | Scarpa Funk Shoes (Female) | rec |
| 515 | Zapato Dichromatic Boots (Female) | rec |
| 516 | Zapato Classic Boots (Female) | rec |
| 517 | Zapato Studded Boots (Female) | rec |
| 518 | Zapato Light Ankle Boots (Female) | rec |
| 519 | Zapato Dark Ankle Boots (Female) | rec |
| 520 | Esporte All-Terrain Shoes (Male) | rec |
| 521 | Esporte Comfort Shoes (Male) | rec |
| 522 | Esporte Hiking X Shoes (Male) | rec |
| 523 | Esporte Waterproof Shoes (Male) | rec |
| 524 | Klasse Arrow Ankle Boots (Female) | rec |
| 525 | Esporte Runner Shoes(Female) | rec |
| 526 | Klasse Avenir High Boots (Female) | rec |
| 527 | Klasse Streamline Ankle Boots (Female) | rec |
| 528 | GIS Shoes (Male) | rec |
| 529 | GIS Shoes (Female) | rec |
| 530 | Aquatican Brew | rec |
| 531 | Nano Bite Burger | rec |
| 532 | Pulse Mineral Water | rec |
| 533 | Mystical Cola | rec |
| 534 | Ginnan Sushi | rec |
| 535 | Flying Orange | rec |
| 536 | DoubleCheese Mystique | rec |
| 537 | Orange Easter Egg | rec |
| 538 | Red Easter Egg | rec |
| 539 | Fake Easter Egg | rec |
| 540 | Yellow Easter Egg | rec |
| 541 | Colonial Easter Egg | rec |
| 542 | Blue Lock Box | rec |
| 543 | Green Lock Box | rec |
| 544 | Red Lock Box | rec |
| 545 | Gray Lock Sphere | rec |
| 546 | Teal Lock Box | rec |
| 547 | Yellow Lock Box | rec |
| 548 | Lottery Ticket | rec |
| 549 | SquiglyPig Milk | rec |
| 550 | General Cao's Chicken | rec |
| 551 | Vivo Burrito | rec |
| 552 | Blue Keycard | rec |
| 553 | Green Keycard | rec |
| 554 | Red Keycard | rec |
| 555 | Gray Keycard | rec |
| 556 | Teal Keycard | rec |
| 557 | Yellow Keycard | rec |
| 580 | Tetrahydrocannabinol | rec |
| 581 | Amyl Nitrite | rec |
| 582 | Butyl Nitrite | rec |
| 583 | Dexedrine | rec |
| 584 | Biphetamin | rec |
| 585 | Neurotonin | rec |
| 586 | Polycodeine | rec |
| 587 | Methedrine | rec |
| 588 | Polydichloric Euthemal | rec |
| 589 | Oxazoline | rec |
| 590 | Opiatech | rec |
| 591 | Phencyclidine | rec |
| 592 | MDMA | rec |
| 593 | Neoamphetamine | rec |
| 594 | Benzedrine | rec |
| 595 | Desoxyn | rec |
| 596 | Ritalin | rec |
| 597 | X-Dopamine | rec |
| 598 | Cocaboline | rec |
| 599 | Anabolica | rec |
| 610 | Workout T-Shirt (Male) | rec |
| 611 | Exercise T-Shirt (Male) | rec |
| 612 | Day T-Shirt (Male) | rec |
| 613 | Shadows T-Shirt (Male) | rec |
| 614 | Blood T-Shirt (Male) | rec |
| 615 | Initiation T-Shirt (Male) | rec |
| 616 | Hiring T-Shirt (Male) | rec |
| 617 | Gold Brand T-Shirt (Male) | rec |
| 618 | Gray Brand T-Shirt (Male) | rec |
| 619 | Neon Brand T-Shirt (Male) | rec |
| 620 | All-weather T-Shirt (Male) | rec |
| 621 | Squad T-Shirt (Male) | rec |
| 622 | Nature T-Shirt (Male) | rec |
| 623 | Style T-Shirt (Male) | rec |
| 624 | Gateway T-Shirt (Male) | rec |
| 625 | Contract T-Shirt (Male) | rec |
| 626 | Celebration T-Shirt (Male) | rec |
| 627 | Civilian NeoPunk Shirt (Male) | alias |
| 628 | Civilian Space Golf Attire (Male) | alias |
| 629 | Civilian Office casual (Male) | alias |
| 630 | Trainee Jacket (Male) | rec |
| 631 | Patrolman's Jacket (Male) | rec |
| 632 | Street Judge Jacket (Male) | rec |
| 633 | Dominion Fatigue (Male) | rec |
| 634 | Viper Assault Jacket (Male) | rec |
| 635 | Ryubusa Jacket (Male) | rec |
| 636 | Turtle-dove Jacket (Male) | rec |
| 637 | Emerald Dove Jacket (Male) | rec |
| 638 | Penance Jacket (Male) | rec |
| 639 | Leaders 47 Special (Male) | rec |
| 640 | Commando Jacket (Male) | rec |
| 641 | Sand Burn Jacket (Male) | rec |
| 642 | Canary Jacket (Male) | rec |
| 643 | Prospectors Rock (Male) | rec |
| 644 | ALM Jacket (Male) | rec |
| 645 | Zivce6 Jacket (Male) | rec |
| 646 | Infinity Jacket (Male) | rec |
| 647 | Amplitude Jacket (Male) | rec |
| 648 | Blue Patrol Jacket (Male) | rec |
| 649 | Green Patrol Jacket (Male) | rec |
| 650 | Protester Jacket (Male) | rec |
| 651 | Shadow Wrap (Male) | rec |
| 652 | Stylized Jacket (Male) | rec |
| 653 | Gray Defense Jacket (Male) | rec |
| 654 | Neon Defense Jacket (Male) | rec |
| 655 | Civilian Adama Jacket (Male) | alias |
| 656 | Civilian Menasco Sports Jacket (Male) | alias |
| 657 | Civilian Midnight Black Jacket (Male) | alias |
| 658 | Civilian Gentleman's Jacket (Male) | alias |
| 670 | Patrolman Trenchcoat (Male) | rec |
| 671 | Soldier Trenchcoat (Male) | rec |
| 672 | Anarchy Trenchcoat (Male) | rec |
| 673 | Riot Trenchcoat (Male) | rec |
| 674 | Assassin Trenchcoat (Male) | rec |
| 675 | Miner Trenchcoat (Male) | rec |
| 676 | Production Trenchcoat (Male) | rec |
| 677 | Explorers Trenchcoat (Male) | rec |
| 678 | Technician Trenchcoat (Male) | rec |
| 679 | Protector Trenchcoat (Male) | rec |
| 680 | Assault Trenchcoat (Male) | rec |
| 681 | Mankind Trenchcoat (Male) | rec |
| 682 | Trenchcoat of the Shadows (Male) | rec |
| 683 | Utility Trenchcoat (Male) | rec |
| 684 | Colonization Trenchcoat (Male) | rec |
| 685 | Neon Technician Trenchcoat (Male) | rec |
| 686 | Civilian Enfield Overcoat (Male) | alias |
| 690 | Defender Robe (Male) | rec |
| 691 | Elder Robe (Male) | rec |
| 692 | Luxury Robe (Male) | rec |
| 693 | Robe of Shadows (Male) | rec |
| 694 | Deep Mines Duster (Male) | rec |
| 695 | Gray Robe (Male) | rec |
| 700 | Patrol Trousers (Male) | rec |
| 701 | Liberation Trousers (Male) | rec |
| 702 | Diplomatic Trousers (Male) | rec |
| 703 | Riot Pants (Male) | rec |
| 704 | Chaotic Pants (Male) | rec |
| 705 | Hitman Trousers (Male) | rec |
| 706 | Battle Trousers (Male) | rec |
| 707 | Mining Trousers (Male) | rec |
| 708 | Producer Pants (Male) | rec |
| 709 | Technician Trousers (Male) | rec |
| 710 | Standard Patrol Trousers (Male) | rec |
| 711 | Casual Trousers (Male) | rec |
| 712 | Protector Jeans (Male) | rec |
| 713 | Red Spandex Pants (Male) | rec |
| 714 | Assault Pants (Male) | rec |
| 715 | Executive Trousers (Male) | rec |
| 716 | Workers’ Pants (Male) | rec |
| 717 | Analytical Trousers (Male) | rec |
| 718 | Civilian PermaLast T5-Enhanced Trousers (Male) | alias |
| 719 | Civilian Thick Anti Freeze Trousers (Male) | alias |
| 720 | Casual Trousers (Male) | rec |
| 721 | Rugged Trousers (Male) | rec |
| 722 | Anti-Riot Pants (Male) | rec |
| 723 | Tactical Trousers (Male) | rec |
| 724 | Assault Trousers (Male) | rec |
| 725 | Raid Trousers (Male) | rec |
| 726 | Sentinel Pants (Male) | rec |
| 727 | Light Defender Trousers (Male) | rec |
| 728 | Anarchy Trousers (Male) | rec |
| 729 | Executioner Trousers (Male) | rec |
| 730 | Predator Trousers (Male) | rec |
| 731 | Hiking Trousers (Male) | rec |
| 732 | Miners’ Pants (Male) | rec |
| 733 | Aurelian Combat Trousers (Male) | rec |
| 734 | Gray Executive Trousers (Male) | rec |
| 735 | Management Trousers (Male) | rec |
| 736 | Maintenance Trousers (Male) | rec |
| 737 | Innovator Trousers (Male) | rec |
| 738 | Service Trousers (Male) | rec |
| 739 | Reconnaissance Trousers (Male) | rec |
| 740 | Arbre Trousers (Male) | rec |
| 741 | Havoc Trousers (Male) | rec |
| 742 | Legionnaire Trousers (Male) | rec |
| 743 | Mijnwerker Trousers(Male) | rec |
| 744 | Secretarial Trousers (Male) | rec |
| 745 | Neon Research Trousers (Male) | rec |
| 746 | Civilian Sierra 17 Trousers (Male) | alias |
| 747 | Civilian Keplers Luxurious Trousers (Male) | alias |
| 760 | Blue Tactical Trousers (Male) | rec |
| 761 | Staff Trousers (Male) | rec |
| 762 | Mojo Trousers (Male) | rec |
| 763 | Pharmaceutical Trousers (Male) | rec |
| 764 | Soldner Trousers (Male) | rec |
| 765 | Bergmann Trousers (Male) | rec |
| 766 | Nucleo Trousers (Male) | rec |
| 767 | Marketer’s Trousers (Male) | rec |
| 768 | Molecular Trousers (Male) | rec |
| 780 | Liberty Trousers (Male) | rec |
| 781 | Harmony Trousers (Male) | rec |
| 782 | Casual Trousers (Male) | rec |
| 783 | Civilian Deviates Trousers (Male) | alias |
| 784 | Civilian L.L. Cargo Pants (Male) | alias |
| 785 | Civilian Ultra Secure Trousers (Male) | alias |
| 790 | Blue Tank Top (Female) | rec |
| 791 | Advocate T-Shirt (Female) | rec |
| 792 | Armed Forces T-Shirt (Female) | rec |
| 793 | Militant T-Shirt (Female) | rec |
| 794 | Purity T-Shirt (Female) | rec |
| 795 | Frenzied T-Shirt (Female) | rec |
| 796 | Sinister T-Shirt (Female) | rec |
| 797 | Death Dealer T-Shirt (Female) | rec |
| 798 | Eradicator T-Shirt (Female) | rec |
| 799 | Excavator T-Shirt (Female) | rec |
| 800 | Casual Leather Jacket (Female) | rec |
| 801 | Casual T-Shirt (Female) | rec |
| 802 | Patrolwoman T-Shirt (Female) | rec |
| 803 | Freedom Fatigue Top (Female) | rec |
| 804 | Harmony Top (Female) | rec |
| 805 | Power Top (Female) | rec |
| 806 | Digger Top (Female) | rec |
| 807 | Casual Fridays Top (Female) | rec |
| 808 | Travel Top (Female) | rec |
| 809 | Civilian Unlimited Secret Blouse (Female) | alias |
| 810 | Peace Keeper Jacket (Female) | rec |
| 811 | Blue Knight Jacket (Female) | rec |
| 812 | Cadet Jacket (Female) | rec |
| 813 | Parade Jacket (Female) | rec |
| 814 | Harmony Jacket (Female) | rec |
| 815 | Celestial Jacket (Female) | rec |
| 816 | Jackal Jacket (Female) | rec |
| 817 | Muscle Jacket (Female) | rec |
| 818 | Fury Jacket(Female) | rec |
| 819 | Tunnel Jacket (Female) | rec |
| 820 | Scrubber Jacket (Female) | rec |
| 821 | Luna Jacket (Female) | rec |
| 822 | Gray Defense Jacket (Female) | rec |
| 823 | Gray Assault Jacket (Female) | rec |
| 824 | Neon Defense Jacket (Female) | rec |
| 825 | Neon Assault Jacket (Female) | rec |
| 826 | Blue Defense Jacket (Female) | rec |
| 827 | Army Green Defense Jacket (Female) | rec |
| 828 | Protesters Jacket (Female) | rec |
| 829 | Anarchy Jacket (Female) | rec |
| 830 | Assassin Jacket (Female) | rec |
| 831 | Gray Mining Jacket (Female) | rec |
| 832 | Neon Mining Jacket (Female) | rec |
| 833 | Civilian Aerodynamic Deluxe Shirt (Female) | alias |
| 834 | Civilian PermaLast Primary Jacket (Female) | alias |
| 835 | Civilian Gun Metal Jacket (Female) | alias |
| 836 | Civilian Oriental Designs Jacket (Female) | alias |
| 837 | Civilian Running Jacket (Female) | alias |
| 850 | Blue Patrol Trenchcoat (Female) | rec |
| 851 | Army Green Patrol Trenchcoat (Female) | rec |
| 852 | Chaos Trenchcoat (Female) | rec |
| 853 | Anarchy Trenchcoat (Female) | rec |
| 854 | Assassin Trenchcoat (Female) | rec |
| 855 | Gray Defense Trenchcoat (Female) | rec |
| 856 | Gray Mining Trenchcoat (Female) | rec |
| 857 | Production Trenchcoat (Female) | rec |
| 858 | Neon Mining Trenchcoat (Female) | rec |
| 859 | Protector Trenchcoat (Female) | rec |
| 860 | Assault Trenchcoat (Female) | rec |
| 861 | Phoenix Trenchcoat (Female) | rec |
| 862 | Dark Trench (Female) | rec |
| 863 | Widow Trenchcoat (Female) | rec |
| 864 | Discovery Trenchcoat (Female) | rec |
| 865 | Neon Defense Trenchcoat (Female) | rec |
| 866 | Civilian Hellstorm Trenchcoat (Female) | alias |
| 867 | Civilian Aurelian Designer Trenchcoat (Female) | alias |
| 870 | Protector Robe (Female) | rec |
| 871 | Light Veteran Robe (Female) | rec |
| 872 | Stylized Robe (Female) | rec |
| 873 | Dark Veteran Robe (Female) | rec |
| 874 | Gold Robe (Female) | rec |
| 875 | Gray Robe (Female) | rec |
| 880 | Patrol Skirt (Female) | rec |
| 881 | Defense Skirt (Female) | rec |
| 882 | Soldier Skirt (Female) | rec |
| 883 | Defender Skirt (Female) | rec |
| 884 | Light Skirt (Female) | rec |
| 885 | Shadow Skirt (Female) | rec |
| 886 | Dark Skirt (Female) | rec |
| 887 | Assassin Skirt (Female) | rec |
| 888 | Contract Skirt (Female) | rec |
| 889 | Gold Skirt (Female) | rec |
| 890 | Gray Skirt (Female) | rec |
| 891 | Neon Skirt (Female) | rec |
| 892 | Civilian Casual Miniskirt (Female) | alias |
| 893 | Civilian Woven Design Miniskirt (Female) | alias |
| 894 | Civilian Dark Abyss Skirt (Female) | alias |
| 895 | Civilian Plaid Skirt (Female) | alias |
| 900 | Patrolman Trousers (Female) | rec |
| 901 | Blue Defender Trousers (Female) | rec |
| 902 | Defense Trousers (Female) | rec |
| 903 | Assault Trousers (Female) | rec |
| 904 | Protester Trousers (Female) | rec |
| 905 | Light Defender Trousers (Female) | rec |
| 906 | Shadow Trousers (Female) | rec |
| 907 | Assassin Trousers (Female) | rec |
| 908 | Assault Trousers (Female) | rec |
| 909 | Miner Trousers (Female) | rec |
| 910 | Production Trousers (Female) | rec |
| 911 | Gold Defense Trousers (Female) | rec |
| 912 | Miner Trousers (Female) | rec |
| 913 | Gray Defense Trousers (Female) | rec |
| 914 | Technician Trousers (Female) | rec |
| 915 | Neon Defense Trousers (Female) | rec |
| 916 | Blue Protector Trousers (Female) | rec |
| 917 | Veteran Trousers (Female) | rec |
| 918 | Phoenix Trousers (Female) | rec |
| 919 | Anarchy Trousers (Female) | rec |
| 920 | Assassin Trousers (Female) | rec |
| 921 | Brown Colonist Trousers (Female) | rec |
| 922 | Gray Colonist Trousers (Female) | rec |
| 923 | Neon Colonist Trousers (Female) | rec |
| 924 | Civilian Faded Beauty Hip Huggers (Female) | alias |
| 940 | Blue Warden Trousers (Female) | rec |
| 941 | Green Soldier Trousers (Female) | rec |
| 942 | Dealer Trousers (Female) | rec |
| 943 | Rebel Trousers (Female) | rec |
| 944 | Defender Trousers (Female) | rec |
| 945 | Brown Assault Trousers (Female) | rec |
| 946 | Gray Assault Trousers (Female) | rec |
| 947 | Gray Traveler Trousers (Female) | rec |
| 948 | Neon Assault Trousers (Female) | rec |
| 949 | Blue Dominion Trousers (Female) | rec |
| 950 | Green Dominion Trousers (Female) | rec |
| 951 | White Traveler Trousers (Female) | rec |
| 952 | Loyalty Trousers (Female) | rec |
| 953 | Contract Trousers (Female) | rec |
| 954 | Brown Traveler Trousers (Female) | rec |
| 955 | Gray Trousers (Female) | rec |
| 956 | Neon Trousers (Female) | rec |
| 960 | White Trousers (Female) | rec |
| 961 | Diplomatic Trousers (Female) | rec |
| 962 | Miner Trousers (Female) | rec |
| 963 | Stunning Bell Bottom Pants (Female) | alias |
| 964 | Executive Trousers (Female) | alias |
| 973 | Mineral Rock | rec |
| 974 | Mineral Rock | rec |
| 975 | Mineral Rock | rec |
| 976 | Backpack | rec |
| 977 | Backpack | rec |
| 978 | Backpack | rec |
| 979 | Alien Egg | rec |
| 980 | Murder ID Card | rec |
| 981 | Bronze Token | rec |
| 982 | Silver Token | rec |
| 983 | Golden Token | rec |
| 984 | Deployable Shield | rec |
| 985 | Vortex Ticket | rec |
| 986 | Influence Generator | rec |
| 987 | Medical Unit | rec |
| 988 | Mining Rig | rec |
| 989 | Infiltration Hacking Interface | rec |
| 990 | Bypass Hacking Interface | rec |
| 991 | Apartment Identifier | rec |
| 992 | Territory Controller | rec |
| 993 | Salvotech CX-90 Explosive Charge | rec |
| 994 | Vortex Reactor | rec |
| 995 | Production Manager | rec |
| 996 | Multicom Access Point | rec |
| 997 | Market Vendor | rec |
| 998 | Slot Machine | rec |
| 999 | Transport Storage | rec |
| 1000 | NeoPunk Shirt (Male) | rec |
| 1001 | Fusion Spider (Male) | rec |
| 1002 | Snakeskin Shirt (Male) | rec |
| 1003 | Adama Jacket (Male) | rec |
| 1004 | Von Neumann Jacket (Male) | rec |
| 1005 | Menasco Sports Jacket (Male) | rec |
| 1006 | Gun Metal Overcoat (Male) | rec |
| 1007 | D.I.R.T. Trenchcoat (Male) | rec |
| 1008 | STS Trenchcoat (Male) | rec |
| 1009 | MegaCorp Shirt (Male) | rec |
| 1010 | Space Golf Attire (Male) | rec |
| 1011 | Office casual (Male) | rec |
| 1012 | Refined Jacket (Male) | rec |
| 1013 | Gentleman's Jacket (Male) | rec |
| 1014 | H.G.Willams Jacket (Male) | rec |
| 1015 | Doberman Trenchcoat (Male) | rec |
| 1016 | Enfield Overcoat (Male) | rec |
| 1017 | Empire Trenchcoat (Male) | rec |
| 1018 | Santa Coat (Male) | rec |
| 1019 | GIS Coat (Male) | rec |
| 1020 | Exile Overcoat (Male) | rec |
| 1021 | Staff Shirt Casual (Male) | rec |
| 1022 | Staff Shirt Formal (Male) | rec |
| 1023 | Swamp Trenchcoat 1 (Male) | rec |
| 1024 | Swamp Trenchcoat 2 (Male) | rec |
| 1025 | Jack-o'-lantern Shirt (Male) | rec |
| 1026 | Flaming Skull Shirt (Male) | rec |
| 1027 | Black Shuck Jacket (Male) | rec |
| 1028 | Echion Jacket (Male) | rec |
| 1029 | Midnight Black Jacket (Male) | rec |
| 1030 | White Lightning Jacket (Male) | rec |
| 1031 | Nightwalker Trenchcoat (Male) | rec |
| 1032 | Black Shadow Trenchoat (Male) | rec |
| 1033 | Vampire Robe (Male) | rec |
| 1034 | Nub Shirt (Male) | rec |
| 1035 | Gingerbread Shirt (Male) | rec |
| 1036 | Cyber Black (Male) | rec |
| 1037 | Shadow's Shirt (Male) | rec |
| 1038 | Black Businessman's Jacket (Male) | rec |
| 1039 | Blue Businessman's Jacket (Male) | rec |
| 1040 | Unlimited Secret Blouse (Female) | rec |
| 1041 | Chromian Designs Blouse (Female) | rec |
| 1042 | Raining Thunder (Female) | rec |
| 1043 | Oriental Designs Jacket (Female) | rec |
| 1044 | Running Jacket (Female) | rec |
| 1045 | Authoritarian Jacket (Female) | rec |
| 1046 | Aurelian Design Trenchcoat (Female) | rec |
| 1047 | White Trenchcoat of Purity (Female) | rec |
| 1048 | Lightning Coat (Female) | rec |
| 1049 | Bejiffy Trendy Shirt (Female) | rec |
| 1050 | Aerodynamic Deluxe Shirt (Female) | rec |
| 1051 | Bejiffy Exquisite Shirt (Female) | rec |
| 1052 | PermaLast Primary Jacket (Female) | rec |
| 1053 | PermaLast Luxury Jacket (Female) | rec |
| 1054 | Airlight Jacket (Female) | rec |
| 1055 | Aurelian Designer Trenchcoat (Female) | rec |
| 1056 | Freedom Nationalist Trenchcoat (Female) | rec |
| 1057 | TermoSecure Trenchcoat (Female) | rec |
| 1058 | Santa Coat (Female) | rec |
| 1059 | GIS Overcoat (Female) | rec |
| 1060 | Exile Overcoat (Female) | rec |
| 1061 | Staff Shirt Casual (Female) | rec |
| 1062 | Jack-o'-lantern Shirt (Female) | rec |
| 1063 | Flaming Skull Shirt (Female) | rec |
| 1064 | Black Shuck Jacket (Female) | rec |
| 1065 | Echion Jacket (Female) | rec |
| 1066 | Stylized Black Blouse (Female) | rec |
| 1067 | Alternate Universal Blouse (Female) | rec |
| 1068 | Gun Metal Jacket (Female) | rec |
| 1069 | Hellstorm Trenchcoat (Female) | rec |
| 1070 | Nurse Blouse (Female) | rec |
| 1071 | Gingerbread Shirt (Female) | rec |
| 1072 | Cyber Black (Female) | rec |
| 1073 | Shadow's Shirt (Female) | rec |
| 1074 | Blue Businesswoman's Jacket (Female) | rec |
| 1075 | Black Businesswoman's Jacket (Female) | rec |
| 1076 | Brown Businesswoman's Jacket (Female) | rec |
| 1077 | Grey Businesswoman's Jacket (Female) | rec |
| 1078 | Pink Businesswoman's Jacket (Female) | rec |
| 1079 | Purple Businesswoman's Jacket (Female) | rec |
| 1080 | PermaLast T5-Enhanced Trousers (Male) | rec |
| 1081 | PermaLast All-Terrain Trousers (Male) | rec |
| 1082 | Thick Anti Freeze Trousers (Male) | rec |
| 1083 | Keplers Luxurious Trousers (Male) | rec |
| 1084 | Prosperous Civilian Trousers (Male) | rec |
| 1085 | Commoners Trousers (Male) | rec |
| 1086 | Deviates Trousers (Male) | rec |
| 1087 | Ultra Secure Trousers (Male) | rec |
| 1088 | Infinite Multi-Purpose Trousers (Male) | rec |
| 1089 | Sierra 17 Trousers (Male) | rec |
| 1090 | Civilian Old War Issued Trousers (Male) | rec |
| 1091 | NYC Urban Trousers (Male) | rec |
| 1092 | Khaki Trousers (Male) | rec |
| 1093 | Green Linen Pants (Male) | rec |
| 1094 | MUD Trousers(Male) | rec |
| 1095 | L.L. Cargo Pants (Male) | rec |
| 1096 | A.K.A. Trousers (Male) | rec |
| 1097 | Relaxed Trousers (Male) | rec |
| 1098 | Santa Trousers (Male) | rec |
| 1099 | GIS Trousers (Male) | rec |
| 1100 | Exile Trousers (Male) | rec |
| 1101 | Black Shuck Trousers (Male) | rec |
| 1102 | Echion Trousers (Male) | rec |
| 1103 | Midnight Black Trousers (Male) | rec |
| 1104 | Shadow's Trousers (Male) | rec |
| 1105 | Black Slacks (Male) | rec |
| 1106 | Blue Slacks (Male) | rec |
| 1107 | Brown Slacks (Male) | rec |
| 1108 | Grey Slacks (Male) | rec |
| 1109 | Pink Slacks (Male) | rec |
| 1110 | White Slacks (Male) | rec |
| 1120 | Casual Miniskirt (Female) | rec |
| 1121 | Woven Design Miniskirt (Female) | rec |
| 1122 | Plaid Skirt (Female) | rec |
| 1123 | Dark Boot Cut Jeans (Female) | rec |
| 1124 | Necarian Adventure Trousers (Female) | rec |
| 1125 | Slim Work Pants (Female) | rec |
| 1126 | Teal Slim Pants (Female) | rec |
| 1127 | Stunning Bell Bottom Pants (Female) | rec |
| 1128 | Hip Hugging Jeans (Female) | rec |
| 1129 | Faded Beauty Jeans (Female) | rec |
| 1130 | Traveling Pants (Female) | rec |
| 1131 | Grey Relaxed Pants (Female) | rec |
| 1132 | Slim Fit Jeans (Female) | rec |
| 1133 | White Washed Jeans (Female) | rec |
| 1134 | Fitted Pants (Female) | rec |
| 1135 | Executive Trousers (Female) | rec |
| 1136 | Faded Beauty Hip Huggers (Female) | rec |
| 1137 | Flare Jeans (Female) | rec |
| 1138 | Santa Trousers (Female) | rec |
| 1139 | GIS Trousers (Female) | rec |
| 1140 | Exile Trousers (Female) | rec |
| 1141 | Black Shuck Trousers (Female) | rec |
| 1142 | Echion Trousers (Female) | rec |
| 1143 | Dark Abyss Skirt (Female) | rec |
| 1144 | Nurse Skirt (Female) | rec |
| 1145 | Shadow's Trousers (Female) | rec |
| 1146 | Blue Businesswoman's Trousers (Female) | rec |
| 1147 | Blue Businesswoman's Skirt (Female) | rec |
| 1148 | Black Businesswoman's Trousers (Female) | rec |
| 1149 | Black Businesswoman's Skirt (Female) | rec |
| 1151 | Canister (Anthracite) | rec |
| 1152 | Canister (Bauxite) | rec |
| 1153 | Canister (Beryllium) | rec |
| 1154 | Canister (Caoutchouc) | rec |
| 1155 | Canister (Chemical Substances) | rec |
| 1156 | Canister (Chrome) | rec |
| 1157 | Canister (Coal) | rec |
| 1158 | Canister (Cobalt) | rec |
| 1159 | Canister (Copper) | rec |
| 1160 | Canister (Diamonds) | rec |
| 1161 | Canister (Gold) | rec |
| 1162 | Canister (Iridium) | rec |
| 1163 | Canister (Iron) | rec |
| 1164 | Canister (Manganese) | rec |
| 1165 | Canister (Crude Oil) | rec |
| 1166 | Canister (Nickel) | rec |
| 1167 | Canister (Organic Material) | rec |
| 1168 | Canister (Plasma) | rec |
| 1169 | Canister (Platinum) | rec |
| 1170 | Canister (Silicon) | rec |
| 1171 | Canister (Silver) | rec |
| 1172 | Canister (Titanium) | rec |
| 1173 | Canister (Vanadium) | rec |
| 1174 | Canister (Water) | rec |
| 1175 | Canister (Tungsten) | rec |
| 1176 | Canister (Uraninite) | rec |
| 1177 | Canister (Lithium) | rec |
| 1194 | Aluminium | rec |
| 1195 | Titanium Alloy | rec |
| 1196 | Metal Alloy | rec |
| 1197 | Special Steel Alloy | rec |
| 1198 | Chemicals | rec |
| 1199 | Carbon | rec |
| 1200 | Rubber | rec |
| 1201 | Bioplasma | rec |
| 1202 | Plastics | rec |
| 1203 | Electronic Components | rec |
| 1204 | CPU | rec |
| 1205 | Glass | rec |
| 1206 | Carbon Fiber | rec |
| 1207 | Professional Electronics | rec |
| 1208 | High Performance Conductor | rec |
| 1209 | Conductor | rec |
| 1210 | Plastic Syntactic Foam | rec |
| 1211 | Titanium Syntactic Foam | rec |
| 1212 | Textiles | rec |
| 1213 | Carcinus Forelimbs | rec |
| 1214 | Carcinus Cranium | rec |
| 1215 | Carcinus Dorsal Carapace | rec |
| 1216 | Behemoth Forelimbs | rec |
| 1217 | Behemoth Cranium | rec |
| 1218 | Behemoth Tail | rec |
| 1219 | Sturdy Carapace | rec |
| 1220 | Piercing Oculi | rec |
| 1221 | Pulsating Cerebrum | rec |
| 1222 | Serrated Claws | rec |
| 1223 | Mantis Cranium | rec |
| 1224 | Mantis Metathorax | rec |
| 1225 | Mantis Mesothorax | rec |
| 1226 | Sticky Xenomorphic Fluid | rec |
| 1227 | Solid Carbonite Shell | rec |
| 1228 | Xenomorph Proteins | rec |
| 1229 | Ultra Resilient Mineral | rec |
| 1230 | Deuterium Water | rec |
| 1231 | Xenomorph Hemolymph | rec |
| 1232 | Bloody Xenomorph Slime | rec |
| 1233 | Xenomorph Endocrine Glands | rec |
| 1234 | Plutonium | rec |
| 1235 | Aluminium Alloy | rec |
| 1236 | Ceramic | rec |
| 1237 | Artificial Logic Device | rec |
| 1238 | Battery Cell | rec |
| 1280 | Production Schematic | rec |
| 1281 | Production Mod | rec |
| 1300 | PNS Data Cube | rec |
| 1301 | Data Cube | rec |
| 1600 | Jack-o'-lantern Hat | rec |
| 1601 | Snowman Hat | rec |
| 1602 | Camouflage Hat | rec |
| 1603 | Bunny Ears | rec |
| 1604 | Top Hat | rec |
| 1605 | Fedora | rec |
| 1606 | Halo | rec |
| 1607 | Horns | rec |
| 1608 | Xabinius' Crown | rec |
| 1609 | Field Cap | rec |
| 1800 | Character Designer | rec |
| 1801 | Name Change Contract | rec |
| 1802 | Faction Ownership Contract | rec |
| 1803 | Faction Name Change Contract | rec |
| 1804 | Elementum | rec |
| 1805 | Dice | rec |
| 1806 | Skill Timer | rec |
| 1807 | Skill Respec | rec |
| 1808 | Coin Voucher | rec |
| 1809 | Skill Book | alias |
| 1820 | Brown Businessman's Jacket (Male) | rec |
| 1821 | Grey Businessman's Jacket (Male) | rec |
| 1822 | Pink Businessman's Jacket (Male) | rec |
| 1823 | White Businessman's Jacket (Male) | rec |
| 1824 | Peace T-Shirt (Male) | rec |
| 1825 | Street Gang Jacket (Male) | rec |
| 1826 | Investigators Trenchcoat (Male) | rec |
| 1827 | American Enterprises Trench (Male) | rec |
| 1828 | Horse Comic's T-Shirt (Male) | rec |
| 1829 | Robe of Multi-Religions (Male) | rec |
| 1830 | Club Trench (Male) | rec |
| 1831 | Lab Coat (Male) | rec |
| 1832 | Backer T-Shirt (Male) | rec |
| 1960 | Peace T-Shirt (Female) | rec |
| 1961 | Street Gang Shirt (Female) | rec |
| 1962 | Inspectors Trenchcoat (Female) | rec |
| 1963 | American Enterprises Trench (Female) | rec |
| 1964 | Horse Comic's T-Shirt (Female) | rec |
| 1965 | Robe of Multi-Religions (Female) | rec |
| 1966 | Club Shirt (Female) | rec |
| 1967 | Lab Coat (Female) | rec |
| 1968 | Backer T-Shirt (Female) | rec |
| 2060 | Street Gang Pants (Male) | rec |
| 2061 | Investigators Trousers (Male) | rec |
| 2062 | Club Pants (Male) | rec |
| 2063 | Backer Pants (Male) | rec |
| 2064 | Incognito Shoes (Male) | rec |
| 2160 | Brown Businesswoman's Trousers (Female) | rec |
| 2161 | Brown Businesswoman's Skirt (Female) | rec |
| 2162 | Grey Businesswoman's Trousers (Female) | rec |
| 2163 | Grey Businesswoman's Skirt (Female) | rec |
| 2164 | Pink Businesswoman's Trousers (Female) | rec |
| 2165 | Pink Businesswoman's Skirt (Female) | rec |
| 2166 | Purple Businesswoman's Trousers (Female) | rec |
| 2167 | Purple Businesswoman's Skirt (Female) | rec |
| 2168 | Street Gang Shorts (Female) | rec |
| 2169 | Inspectors Pants (Female) | rec |
| 2170 | Club Skirt (Female) | rec |
| 2171 | Backer Skirt (Female) | rec |
| 2172 | Incognito Shoes (Female) | rec |
| 2980 | Aramid Altered Gloves (Male) | rec |
| 2981 | Infensus Essentials Gloves (Female) | rec |
| 2982 | Infensus Essentials Gloves (Male) | rec |
| 2983 | Backer Gloves (Female) | rec |
| 2984 | Backer Gloves (Male) | rec |
| 3000 | Emergency Lockout | alias |
| 3001 | Moderate Lockout | alias |
| 3002 | Standard Lockout | alias |
| 3003 | Nocturnal Vision | rec |
| 3004 | Electromyographic Regulator | rec |
| 3005 | Nano-Skeletal Inhibitor | rec |
| 3006 | Calcaneus Booster | rec |
| 3007 | Ultrasonic Obstructor | rec |
| 3008 | Telescopic Interactor | rec |
