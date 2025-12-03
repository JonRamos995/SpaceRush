using UnityEngine;
using System.Collections.Generic;
using SpaceRush.Systems;
using SpaceRush.Models; // For SaveData

namespace SpaceRush.Models
{
    // --- Definition ---
    [System.Serializable]
    public class MetaUpgradeDefinition
    {
        public string ID;
        public string Name;
        public string Description;
        public float Cost; // In Nanites
        public MetaUpgradeEffect Effect;
        public List<EffectData> EffectDataList = new List<EffectData>();

        public MetaUpgradeDefinition(string id, string name, string desc, float cost)
        {
            ID = id;
            Name = name;
            Description = desc;
            Cost = cost;
        }

        public MetaUpgradeDefinition() {}
    }

    // --- Effects ---
    public abstract class MetaUpgradeEffect : ScriptableObject
    {
        public abstract void Apply(GameSaveData data);
        public abstract void ApplyRuntime(); // For static bonuses applied on load
    }

    [CreateAssetMenu(fileName = "MetaStatMultiplier", menuName = "SpaceRush/Meta/Effects/StatMultiplier")]
    public class MetaStatMultiplierEffect : MetaUpgradeEffect
    {
        public string StatName; // e.g., "GlobalMiningSpeed"
        public float Multiplier; // e.g., 1.5

        public override void Apply(GameSaveData data)
        {
            // Does nothing to save data structure, but affects runtime calculations
        }

        public override void ApplyRuntime()
        {
             if (CivilizationManager.Instance != null)
             {
                 CivilizationManager.Instance.AddGlobalMultiplier(StatName, Multiplier);
             }
        }
    }

    [CreateAssetMenu(fileName = "MetaRetention", menuName = "SpaceRush/Meta/Effects/Retention")]
    public class MetaRetentionEffect : MetaUpgradeEffect
    {
        public float PercentageToKeep; // 0.1 for 10%

        public override void Apply(GameSaveData data)
        {
            // Logic to keep resources during reset
            // This is tricky because ResetGameKeepCivilization creates a NEW SaveData or wipes existing managers.
            // But we can modify the "Before Reset" logic.

            // Actually, `ResetGameKeepCivilization` calls `ResetData` on managers.
            // To support retention, we need `PersistenceManager` to ask `CivilizationManager` "What should I keep?"
            // OR `CivilizationManager` can intervene.

            // Let's assume PersistenceManager passes the NEW (empty) data to Apply() after reset?
            // No, Apply(data) implies modifying data.

            // Better approach: CivilizationManager has a method `ApplyRetention(ResourceManager manager)` called BEFORE reset.
        }

        public override void ApplyRuntime()
        {
            // Register retention capability
            if (CivilizationManager.Instance != null)
            {
                CivilizationManager.Instance.RegisterRetention(PercentageToKeep);
            }
        }
    }
}
