# World Login Handoff

How the client moves from the master server into a world server (e.g. spawning
into Manhattan) after [[Login Handshake]] succeeds. The key surprise: the client
does **not** connect to the world server on its own — it asks the **master** to
authorize a transfer, and the master replies with the world server's address for
the client to connect to, while separately telling the world server to expect it.

Packets (`FOM::Packets`, in `CShell.dll`):

| Packet | ID | Dir | Purpose |
| --- | --- | --- | --- |
| `ID_WORLD_LOGIN` | 114 | C→**Master** | request transfer into a world |
| `ID_WORLD_LOGIN_RETURN` | 115 | Master→C | status + the world server's IP:port |

## Flow

```
(after ID_LOGIN_RETURN gives loginWorldId)
Client  --ID_WORLD_LOGIN(worldId, nodeId, playerId, constant)-->  Master
                                                  Master validates + orchestrates:
                                                   - notify destination world (PlayerMigrateWorld)
                                                   - notify prior world (PlayerLeavingWorld), if any
Client  <--ID_WORLD_LOGIN_RETURN(status, worldId, serverIp, serverPort)--  Master
        (on SUCCESS) connect to serverIp:serverPort  -- the world server
```

## `ID_WORLD_LOGIN` (request, 114)

`type /FOM/Packets/Packet_ID_WORLD_LOGIN` — body after the
[[Packet Transport|VariableSizedPacket]] base (`+0x430`):

| Off | Field | Type | Notes |
| --- | --- | --- | --- |
| `0x430` | `worldId` | u8 | target world (the `loginWorldId` from [[Login Handshake]], or a chosen world) |
| `0x431` | `nodeId` | u8 | spawn/transition node within the world |
| `0x434` | `playerId` | u32 | the logging-in player id |
| `0x438` | `constant` | u32 | fixed `1293394` (`0x13BFD2`) — a protocol magic/version guard |

Built and sent by `FUN_101c0e10` (`CShell.dll` rva `0x1c0e10`; it is the sole
caller of the `Packet_ID_WORLD_LOGIN` ctor at rva `0x1bfe00`). Sent to the
**MASTER** destination (see [[Packet Transport]]).

## `ID_WORLD_LOGIN_RETURN` (reply, 115)

`type /FOM/Packets/Packet_ID_WORLD_LOGIN_RETURN` — body at `+0x430`:

| Off | Field | Type |
| --- | --- | --- |
| `0x430` | `status` | `WorldLoginReturnStatus` (u8) |
| `0x431` | `worldId` | u8 |
| `0x434` | `serverIp` | u32 (IPv4) |
| `0x438` | `serverPort` | u16 |

`WorldLoginReturnStatus`: `INVALID=0`, `SUCCESS=1`, `SERVER_OFFLINE=2`,
`WRONG_FACTION=3`, `WORLD_FULL=4`, `UNKNOWN_ERROR=5`, `NO_FACTION_PRIVILEGES=6`,
`OUT_OF_RANGE=7`, `RETRY=8`.

## Client handling — `HandlePacket_ID_WORLD_LOGIN_RETURN` (rva `0x18e340`)

Decompiled behaviour by status:

- **SUCCESS** → `FUN_101c0d60(netMgr, worldId, serverIp, serverPort)` — the
  world-connect routine: the client opens a RakNet connection to
  `serverIp:serverPort` (the world server) and proceeds to enter the world.
  This is where the actual client→world UDP connection is established.
- **RETRY** → reschedules the request after a delay (`FUN_1018c570(now + delay)`)
  — used while the world server is still preparing the migration.
- **SERVER_OFFLINE / WORLD_FULL / WRONG_FACTION / NO_FACTION_PRIVILEGES /
  OUT_OF_RANGE / unknown** → display a localized error (string-table ids
  `0x6bb`/`0x6bc`/`0x6c6`/`0x6c7`/`0x6cb`/`0x6ba`) and reset
  (`FUN_101c0a60(5)`).

So the world server's reachable address is **chosen by the master and handed to
the client** in the RETURN — the client never derives it locally (it does *not*
compute `61000 + worldId` itself; that formula lives server-side in
`ServerConstants.GetWorldClientPort`).

## How session identity is established

The master ties the request to an account by the client's **network address**:

- `WorldLoginHandler` looks up the client `session` in the `ClientRegistry` by
  sender address (a session created during [[Login Handshake]]). A request from
  an unregistered address is dropped.
- It requires `session.PlayerId == packet.playerId` — a client can only world-log
  its own logged-in player; a mismatch returns `UnknownError`.
- It verifies the player row exists and the destination world is registered/online
  (`ServerOffline` otherwise).
- It then `BeginWorldTransfer(worldId)` and forwards the handoff to the world
  servers: `PlayerMigrateWorld { PlayerId, ClientBinaryAddress }` to the
  **destination** world server (so the world server can authorize the incoming
  client by its address), and `PlayerLeavingWorld { PlayerId }` to the **prior**
  world server if the player was already in one.

> Server-side detail (master is C#): in the current emulator code the success
> `WorldLoginReturn` (carrying `serverIp`/`serverPort`) is **not** emitted by
> `WorldLoginHandler` itself — that handler only sends the *error* returns
> directly; the success return's emitter (likely after the destination world
> confirms the migration) is **unverified / possibly in-flux**. The client,
> however, unambiguously expects `serverIp:serverPort` in the SUCCESS return.

## Reproduce

```bash
fomre type /FOM/Packets/Packet_ID_WORLD_LOGIN
fomre type /FOM/Packets/Packet_ID_WORLD_LOGIN_RETURN
fomre type /FOM/Enums/Packets/WorldLoginReturnStatus
fomre decompile "CShell.dll:0x1018e340"   # HandlePacket_ID_WORLD_LOGIN_RETURN
fomre decompile "CShell.dll:0x101c0e10"   # builds/sends ID_WORLD_LOGIN
fomre xref     "CShell.dll:0x101bfe00"    # who constructs the request
```

See [[Login Handshake]] (the master login that precedes this), [[Packet
Transport]] (the envelope + destination routing), and [[Client Architecture]].
