# String Table Lookup

Use this skill to identify or search for string resource IDs in the Face of Mankind client string table (`CRes.dll`).

## When to Use

- When you want to search for a string in order to find associated code.
- When you encounter a string resource ID in disassembly (e.g., `LoadString(5681)`, `(*g_pLTClient->vftable->GetStringHandle)(g_pLTClient, 5681);`).
- When you need to identify what UI text or error message corresponds to a numeric ID.
- When analyzing packet handlers that reference string IDs for messages.

String resources are stored in `segments/` as CSV files. Each CSV has columns: `ID,String`.

## Forward Lookup Procedure

To find a string by ID:

1. Calculate the range start: `start = floor(ID / 50) * 50`.
2. Construct filename: `{start:05d}-{start+49:05d}.csv` (zero-padded to 5 digits).
   - Example: ID 5681 → `floor(5681/50)*50 = 5650` → `05650-05699.csv`.
3. Read `segments/{filename}` directly - do NOT search for the file.
4. Find the row with matching ID in the CSV.

If the file doesn't exist, the ID has no string entry in the table.

## Reverse Lookup Procedure

To find a place in the code that uses a string resource:

1. Use the `grep` tool to find string resource IDs that match your search criteria.
2. Search through the code for `(*g_pLTClient->vftable->GetStringHandle)(g_pLTClient, stringId);` function calls.

## Special Ranges

Some ID ranges have special meaning:

| Range | Purpose |
|-------|---------|
| 17900-17950 | Skill categories |
| 17951-19000 | Skill names (ID 0 = string 17951) |
| 19001-19999 | Skill descriptions |
| 20051-20999 | Skill effects |
| 30000-38999 | Item names (type 0 = string 30000) |
| 40000-48999 | Item descriptions |

For items and skills, see [`docs/items.md`](../../../docs/items.md) and [`docs/skills.md`](../../../docs/skills.md) for human-readable tables.
