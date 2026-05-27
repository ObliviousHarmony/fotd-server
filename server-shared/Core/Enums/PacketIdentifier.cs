namespace FOMServer.Shared.Core.Enums
{
    /// <summary>
    /// The packet identifiers used in the FOM server-client communication.
    /// </summary>
    /// <remarks>
    /// These MUST be kept up-to-date with `fom-network/include/fom-network/PacketIdentifier.h`
    ///
    /// For each new packet added:
    ///
    /// - Core/Packets/<PacketType>.cs: Add a new struct representing the packet data.
    /// - Server-Specific or Shared: Application/Handlers/<PacketType>Handler.cs: Create a packet handler.
    ///
    /// </remarks>
    public enum PacketIdentifier : byte
    {
        // These are the internal packet Ids that cross the interop boundary.
        ID_CONNECTION_REQUEST_ACCEPTED = 14, // Connection request to the server has been accepted.
        ID_CONNECTION_ATTEMPT_FAILED = 15, // Could not connect to the server.
        ID_ALREADY_CONNECTED = 16, // Attempted to connect to a server we're already connected to.
        ID_NEW_INCOMING_CONNECTION = 17, // Connection has been made successfully.
        ID_NO_FREE_INCOMING_CONNECTIONS = 18, // Server is not accepting new connections.
        ID_DISCONNECTION_NOTIFICATION = 19, // The connected system has shut down.
        ID_CONNECTION_LOST = 20, // The connection has been closed.
        ID_RSA_PUBLIC_KEY_MISMATCH = 21, // Current RSA public key does not match what the destination expected.
        ID_CONNECTION_BANNED = 22, // Client is banned by the server.
        ID_INVALID_PASSWORD = 23, // Server has refused the client's password.
        ID_MODIFIED_PACKET = 24, // Packet has been tampered with in transit.

        ID_FOM_PACKET_START = 104,
        ID_REGISTER_WORLD = ID_FOM_PACKET_START,

        // 105
        // 106
        // ID_NOTIFY = 107,
        ID_LOGIN_REQUEST = 108,
        ID_LOGIN_REQUEST_RETURN = 109,
        ID_LOGIN = 110,
        ID_LOGIN_RETURN = 111,
        ID_LOGIN_TOKEN_CHECK = 112,

        // ID_LOGOUT = 113,
        ID_WORLD_LOGIN = 114,
        ID_WORLD_LOGIN_RETURN = 115,

        // ID_WORLD_LOGOUT = 116,
        ID_PLAYER_MIGRATE_WORLD = 117,
        ID_PLAYER_WORLD_READY = 118,
        ID_PLAYER_LEAVING_WORLD = 119,
        ID_REGISTER_CLIENT = 120,
        ID_REGISTER_CLIENT_RETURN = 121,
        ID_CREATE_CHARACTER = 122,

        // ID_VORTEX_GATE = 123,
        ID_CHECK_NAME = 124,
        ID_CHECK_NAME_RETURN = 125,

        // ID_UPDATE = 126,
        // ID_WORLD_UPDATE = 127,
        // ID_WEATHER = 128,
        // ID_ITEMS_REMOVED = 129,
        // ID_ITEMS_CHANGED = 130,
        // ID_ATTRIBUTE_CHANGE = 131,
        // ID_HIT = 132,
        // ID_WORLD_OBJECTS = 133,
        // ID_ATTACK = 134,
        // ID_WEAPONFIRE = 135,
        // ID_ITEM_REMOVED = 136,
        // ID_EXPLOSIVE = 137,
        // ID_MOVE_ITEMS = 138,
        // ID_CHECK_MAIL = 139,
        // ID_MAIL = 140,
        // ID_CHARACTER_UPDATE = 141,
        // ID_NAME_CHANGE = 142,
        // ID_UNLOAD_WEAPON = 143,
        // ID_MERGE_ITEMS = 144,
        // ID_RELOAD = 145,
        // ID_BACKPACK_CONTENTS = 146,
        // ID_ITEMS_ADDED = 147,
        // ID_AVATAR_CHANGE = 148,
        // ID_CHAT = 149,
        // ID_TAUNT = 150,
        // ID_FRIENDS = 151,
        // ID_TRANSFER_ITEMS = 152,
        // ID_STORAGE = 153,
        // ID_MINING = 154,
        // ID_PRODUCTION = 155,
        // ID_MARKET = 156,
        // ID_FACTION = 157,
        // 158
        // ID_PLAYERFILE = 159,
        // 160
        // ID_OBJECT_DETAILS = 161,
        // ID_SPLIT_CONTAINER = 162,
        // ID_SKILLS = 163,
        // ID_USE_ITEM = 164,
        // ID_WORLDSERVICE = 165,
        // ID_HACKING = 166,
        // 167
        // ID_CHEMICAL_LAB = 168,
        // ID_MISSION = 169,
        // ID_PLAYER2PLAYER = 170,
        // 171
        // ID_DEPLOY_ITEM = 172,
        // ID_REPAIR_ITEM = 173,
        // ID_RECYCLE_ITEM = 174,
        // ID_APARTMENTS = 175,
        // ID_SCAN = 176,
        // ID_BOUNTY = 177,
        // ID_ARREST = 178,
        // ID_GAMEMASTER = 179,
        // 180
        // ID_NPC = 181,
        // ID_TERRITORY = 182,
        // 183
        // 184
        // 185
        // 186
        // 187
        // 188
        // 189
        // 190
        // 191
        // 192
        // 193
        // 194
        // 195
        // 196
        // 197
        // 198
        // 199
        // 200
        // 201
        // 202
        // 203
        // 204
        // 205
        // 206
        // 207
        // 208
        // 209
        // 210
        // 211
        // 212
        // 213
        // 214
        // 215
        // 216
        // 217
        // 218
        // 219
        // 220
        // 221
        // 222
        // 223
        // 224
        // 225
        // 226
        // 227
        // 228
        // 229
        // 230
        // 231
        // 232
        // 233
        // 234
        // 235
        // 236
        // 237
        // 238
        // 239
        // 240
        // 241
        // 242
        // 243
        // 244
        // 245
        // 246
        // 247
        // 248
        // 249
        // 250
        // 251
        // 252
        // 253
        // 254
        // 255
    }
}
