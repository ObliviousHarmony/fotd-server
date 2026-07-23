#pragma once

#include <fom-network/structs/player/PlayerSkillsInterop.h>

#include "../InteropTypeSerializer.h"

namespace FOMNetwork {

class PlayerSkillsInteropSerializer
    : protected InteropTypeSerializer<PlayerSkillsInterop> {
 public:
  void Write(RakNet::BitStream& bs, const PlayerSkillsInterop& data) const {
    uint32_t count = data.count;
    if (count > Enum::NUM_SKILL_TYPES) count = Enum::NUM_SKILL_TYPES;

    bs.WriteCompressed(data.trainingMultiplier);
    bs.WriteCompressed(data.combatTrainingMultiplier);
    bs.WriteCompressed(data.ecoTrainingMultiplier);
    bs.WriteCompressed(data.techTrainingMultiplier);
    bs.WriteCompressed(data.unknown1);
    bs.WriteCompressed(count);

    for (uint32_t i = 0; i < count; ++i) {
      bs.WriteCompressed(data.skills[i].id);
      bs.WriteCompressed(data.skills[i].level);
      bs.WriteCompressed(data.skills[i].trainingTime);
      bs.WriteCompressed(data.skills[i].isTraining);
      bs.WriteCompressed(data.skills[i].unknown1);
      bs.WriteCompressed(data.skills[i].unknown2);
    }
  }

  bool Read(RakNet::BitStream& bs, PlayerSkillsInterop& data) const {
    if (!bs.ReadCompressed(data.trainingMultiplier)) return false;
    if (!bs.ReadCompressed(data.combatTrainingMultiplier)) return false;
    if (!bs.ReadCompressed(data.ecoTrainingMultiplier)) return false;
    if (!bs.ReadCompressed(data.techTrainingMultiplier)) return false;
    if (!bs.ReadCompressed(data.unknown1)) return false;
    if (!bs.ReadCompressed(data.count)) return false;
    if (data.count > Enum::NUM_SKILL_TYPES) return false;

    for (uint32_t i = 0; i < data.count; ++i) {
      if (!bs.ReadCompressed(data.skills[i].id)) return false;
      if (!bs.ReadCompressed(data.skills[i].level)) return false;
      if (!bs.ReadCompressed(data.skills[i].trainingTime)) return false;
      if (!bs.ReadCompressed(data.skills[i].isTraining)) return false;
      if (!bs.ReadCompressed(data.skills[i].unknown1)) return false;
      if (!bs.ReadCompressed(data.skills[i].unknown2)) return false;
    }

    return true;
  }
};

}  // namespace FOMNetwork
