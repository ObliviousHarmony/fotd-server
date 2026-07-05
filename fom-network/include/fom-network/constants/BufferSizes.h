#pragma once

#include <cstdint>

namespace FOMNetwork {
namespace BufferSizes {

constexpr int32_t USERNAME = 19;
constexpr int32_t PLAYER_NAME = 20;
constexpr int32_t PLAYER_BIOGRAPHY = 511;
constexpr int32_t FACTION_NAME = 32;
constexpr int32_t RANK_NAME = 32;
constexpr uint32_t MAX_ITEM_LIST_SIZE = 60000;
constexpr int32_t NUM_ITEM_BALANCE_SLIDERS = 4;

}  // namespace BufferSizes
}  // namespace FOMNetwork
