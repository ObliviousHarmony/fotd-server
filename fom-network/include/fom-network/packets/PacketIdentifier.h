#pragma once

#include <fom-network/Common.h>
#include <raknet/MessageIdentifiers.h>

namespace FOMNetwork {

/**
 * For every packet identifier, you must also update:
 *
 * - include/fom-network/packets/<PacketName>.h: Requires a packet struct.
 * - include/fom-network/FOMPacket.h: Add packet struct to union.
 * - include/fom-network/PacketSerializers.h: Requires a serializer declaration.
 * - src/packet-serializers/<PacketName>Serializer.cpp: Requires a serializer
 * implementation.
 * - src/FOMDataSerializer.cpp: Requires adding to the serializer map.
 * - src/NetworkAPI.cpp: Requires adding to the validation map.
 */

/**
 * @enum PacketIdentifier
 *
 * The identifiers used to indicate how a packet should be serialized and
 * deserialized.
 */
enum PacketIdentifier : uint8_t {
  /**
   * This packet ID overlaps with RakNet's ID_INTERNAL_PING.
   * Since the library does not allow sending/receiving
   * this packet, we don't need to worry about overlap.
   */
  ID_FOM_PACKET_READ_ERROR = 0,

  /**
   * ID_USER_PACKET_ENUM is used by RakNet to indicate the
   * FIRST packet id that users are allowed to use.
   */
  ID_FOM_PACKET_START = ID_USER_PACKET_ENUM,

  // 104 ( = ID_FOM_PACKET_START)
  // 105
  // 106
  // ID_NOTIFY = 107,
  ID_LOGIN_REQUEST = 108,
  ID_LOGIN_REQUEST_RETURN = 109,
  ID_LOGIN = 110,
  ID_LOGIN_RETURN = 111,
  // ID_LOGOUT = 112,
  // ID_WORLD_LOGIN = 113,
  // ID_WORLD_LOGIN_RETURN = 114,
  // ID_WORLD_LOGOUT = 115,
  ID_REGISTER_WORLD = 116,
  // 117,
  // 118,
  // ID_REGISTER_CLIENT = 119,
  // ID_REGISTER_CLIENT_RETURN = 120,
  ID_CREATE_CHARACTER = 121,
  ID_CHECK_NAME = 122,
  ID_CHECK_NAME_RETURN = 123,
  // ID_FIND_TARGET = 124,
  // ID_UPDATE = 125,
  // ID_WORLD_UPDATE = 126,
  // ID_CHAT = 127,
  // ID_TAUNT = 128,
  // ID_BACKPACK = 129,
  // ID_BACKPACK_RETURN = 130,
  // ID_ATTACK = 131,
  // ID_ENEMY_ATTACK = 132,
  // ID_HIT = 133,
  // ID_WEATHER = 134,
  // ID_OBJECT_STATUS = 135,
  // ID_OBJECT_CHANGE = 136,
  // ID_CHANGE_OBJECT = 137,
  // ID_ATTRIBUTE_CHANGE = 138,
  // ID_OBJECT_DETAILS = 139,
  // ID_OBJECT_DETAILS_RETURN = 140,
  // ID_DEPARTMENT = 141,
  // ID_DEPARTMENT_DETAILS = 142,
  // ID_FRIENDS = 143,
  // ID_FRIENDS_RETURN = 144,
  // ID_ENEMY_CONTROL = 145,
  // ID_ITEMS_CHANGED = 146,
  // ID_ITEMS_ADDED = 147,
  // ID_ITEMS_REMOVED = 148,
  // ID_ITEM_REMOVED = 149,
  // ID_MOVE_ITEMS = 150,
  // ID_AVATAR_CHANGE = 151,
  // ID_USE_ITEMS = 152,
  // ID_WORLD_OVERVIEW = 153,
  // ID_WORLD_OVERVIEW_RETURN = 154,
  // ID_STORAGE = 155,
  // ID_STORAGE_RETURN = 156,
  // ID_TRANSFER_ITEMS = 157,
  // ID_MINING_PRODUCTION = 158,
  // ID_PRODUCTION_RETURN = 159,
  // ID_PMODS = 160,
  // ID_PMODS_RETURN = 161,
  // ID_BUY_CLONES = 162,
  // ID_BUY_ITEMS = 163,
  // ID_SELL_ITEMS = 164,
  // ID_MARKET = 165,
  // ID_MARKET_BLOCKS_RETURN = 166,
  // ID_MARKET_OFFERS_RETURN = 167,
  // ID_OWN_MARKET_OFFERS = 168,
  // ID_OWN_MARKET_OFFERS_RETURN = 169,
  // ID_RELOAD = 170,
  // ID_DROP_BACKBACKS = 171,
  // 172
  // ID_ADMIN = 173,
  // ID_MAIL = 174,
  // ID_CHECK_MAIL = 175,
  // ID_DEPARTMENTS_RETURN = 176,
  // ID_APARTMENTS = 177,
  // ID_APARTMENTS_RETURN = 178,
  // ID_FACTION = 179,
  // ID_FACTION_STATISTICS = 180,
  // 181
  // ID_FACTION_MEMBERS = 182,
  // ID_PLAYER_FILE = 183,
  // ID_PLAYER_FILE_RETURN = 184,
  // ID_PLAYER2_PLAYER = 185,
  // ID_WORLD_STATS = 186,
  // ID_WORLD_STATS_RETURN = 187,
  // ID_MULTICOM = 188,
  // ID_FACTION_GOALS = 189,
  // ID_OBJECTIVES = 190,
  // ID_MOST_WANTED_LIST = 191,
  // ID_MOST_WANTED_LIST_RETURN = 192,
  // ID_GM = 193,
  // ID_HACKING = 194,
  // ID_GROUP = 195,
  // ID_GROUP_DETAILS = 196,
  // ID_GROUPS_RETURN = 197,
  // ID_EXPLOSIVE = 198,
  // ID_SCAN = 199,
  // ID_FACTION_LOG = 200,
  // ID_VOTING = 201,
  // ID_DONATE = 202,
  // ID_CONTRACT = 203,
  // ID_CONTRACT_DETAILS = 204,
  // ID_CONTRACTS_RETURN = 205,
  // ID_VOLUNTEER = 206,
  // ID_MERGE_ITEMS = 207,
  // ID_LOOT = 208,
  // ID_BUY_PMOD = 209,
  // ID_NPC = 210,
  // ID_NPC_OBJECTS = 211,
  // ID_MESSAGE = 212,
  // ID_MISSION_LOG = 213,
  // ID_UNLOAD_WEAPON = 214,
  // ID_CHARACTER_UPDATE = 215,
  // ID_WARRANT_LIST = 216,
  // ID_NAME_CHANGE = 217,
  // ID_FACTION_POOL = 218,
  // ID_MARKET_PLACE_PERK_UPDATE = 219,
  // ID_FACTION_BLACK_LIST = 220,
  // ID_RESOURCE_MANAGER = 221,
  // ID_CHEMICAL_LAB = 222,
  // ID_PUBLIC_APARTMENTS = 223,
  // ID_DROP_ITEMS = 224,
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
  // 239,
  // 240,
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
};

}  // namespace FOMNetwork
