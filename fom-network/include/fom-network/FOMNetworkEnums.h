#pragma once

namespace FOMNetwork {

/**
 * @enum WorldID
 *
 * The identifiers used for the different world servers in the game.
 */
enum WorldID : uint8_t {
  MASTER_SERVER = 0,
  NYC_MANHATTAN = 1,
  NYC_BROOKLYN = 2,
  TOKYO = 3,
  APARTMENTS = 4,
  TRAINING_GROUND = 5,
  NECARS_FIELD = 6,
  PARIS = 7,
  TESTING = 8,
  BERLIN = 9,
  INVALID = 10,
  ANDROMEDA_CITY = 11,
  // NEW_HAVEN = 12,
  // GANYMEDE = 13,
  DEMORGANS_CASTLE = 14,
  // KEPLERS_DOME = 15,
  // MOON_BASE = 16,
  // STS_GENESIS = 17,
  DSS_YUKON = 18,
  // BOOKERS_VALLEY = 19,
  // EPSILON_ERIDANI = 20,
  // TERRA_VENTURE_I = 21,
  // DOMINION_EXODUS = 22,
  // ESPEN_PARADISE = 23,
  // AQUATICA = 24,
  PEGASI_51 = 25,
  AURELIA = 26,
  PAX_PRIME = 27,
  CERES_DELTA = 28,
  TITAN_STATION = 29,
  CLONING_FACILITY = 30,
  NYC_GROUND_ZERO = 31,
  TRAINING_CENTER = 32,

  NUM_WORLDS = 33
};

/**
 * @enum Faction
 *
 * The identifiers used for the different factions in the game.
 */
enum Faction : uint8_t {
  INVALID = 0,
  LED = 1,
  FDC = 2,
  GOM = 3,
  BOS = 4,
  MOTB = 5,
  CMG = 6,
  EC = 7,
  VI = 8,
  NUM_FACTIONS = 9
};

/**
 * @enum Faction
 *
 * The identifiers used for the different faction relations in the game.
 */
enum FactionRelation : uint8_t {
  INVALID = 0,
  ALLY = 1,
  ECONOMIC_ALLY = 2,
  NEUTRAL = 3,
  ECONOMIC_ENEMY = 4,
  ENEMY = 5,
  NUM_RELATIONS = 6
};

}  // namespace FOMNetwork
