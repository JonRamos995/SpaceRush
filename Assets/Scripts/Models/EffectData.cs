using System;

namespace SpaceRush.Models
{
    [Serializable]
    public class EffectData
    {
        public string EffectType; // e.g. "StatBonus", "UnlockFeature", "BiomeBonus", "StatMultiplier", "Retention"
        public string Target;     // e.g. "MiningSpeed", "REPAIR_DROID", "Volcanic", "GlobalMiningSpeed"
        public float Value;       // e.g. 0.1, 1.0, 0.2
        public string StringValue; // Optional, for string-based values if needed
    }
}
