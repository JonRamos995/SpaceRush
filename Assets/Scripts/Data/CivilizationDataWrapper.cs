using System;
using System.Collections.Generic;

namespace SpaceRush.Data
{
    [Serializable]
    public class CivilizationDataWrapper
    {
        public List<MetaUpgradeConfig> Items;
    }

    [Serializable]
    public class MetaUpgradeConfig
    {
        public string ID;
        public string Name;
        public string Description;
        public float Cost;

        // Effect Configuration
        public string EffectType; // "Retention", "StatMultiplier"
        public string Param1;     // e.g. "GlobalMiningSpeed" (StatName)
        public float Param2;      // e.g. 0.1 (Percentage/Multiplier)
    }
}
