using UnityEngine;
using SpaceRush.Core;
using SpaceRush.Models;
using System.Collections.Generic;
using System.Linq;

namespace SpaceRush.Systems
{
    public class CivilizationManager : MonoBehaviour
    {
        public static CivilizationManager Instance { get; private set; }

        public int Level { get; private set; }
        public float Nanites { get; private set; }

        // Multiplier from Level (Legacy/Base)
        public float BaseMultiplier => 1.0f + (Level * 0.1f);

        // Legacy Property for compatibility
        public float Multiplier => GetGlobalMultiplier("GlobalMiningSpeed");

        // Meta Progression
        private List<MetaUpgradeDefinition> availableUpgrades = new List<MetaUpgradeDefinition>();
        public IReadOnlyList<MetaUpgradeDefinition> AvailableUpgrades => availableUpgrades;

        private HashSet<string> unlockedUpgradeIDs = new HashSet<string>();

        public bool IsUpgradeUnlocked(string upgradeID)
        {
            return unlockedUpgradeIDs.Contains(upgradeID);
        }

        public float GetProjectedAscensionGain()
        {
            // Current formula: 100 per level (Level + 1 because we gain based on next level).
            return (Level + 1) * 100f;
        }

        // Runtime Effects
        private Dictionary<string, float> globalMultipliers = new Dictionary<string, float>();
        private float resourceRetentionPercentage = 0f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeUpgrades();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void InitializeUpgrades()
        {
            // In a real app, load from Resources/GameDatabase.
            // Here we manually create some for the task.

            // 1. Retention Upgrade
            var retDef = ScriptableObject.CreateInstance<MetaUpgradeDefinition>();
            retDef.ID = "RETENTION_1";
            retDef.Name = "Resource Cache";
            retDef.Description = "Keep 10% of resources after Ascension.";
            retDef.Cost = 50f;
            var retEff = ScriptableObject.CreateInstance<MetaRetentionEffect>();
            retEff.PercentageToKeep = 0.1f;
            retDef.Effect = retEff;
            availableUpgrades.Add(retDef);

            // 2. Speed Upgrade
            var spdDef = ScriptableObject.CreateInstance<MetaUpgradeDefinition>();
            spdDef.ID = "GLOBAL_SPEED_1";
            spdDef.Name = "Temporal Flux";
            spdDef.Description = "Increase Global Mining Speed by 50%.";
            spdDef.Cost = 100f;
            var spdEff = ScriptableObject.CreateInstance<MetaStatMultiplierEffect>();
            spdEff.StatName = "GlobalMiningSpeed";
            spdEff.Multiplier = 0.5f; // Additive +50%
            spdDef.Effect = spdEff;
            availableUpgrades.Add(spdDef);
        }

        public void LoadData(CivilizationSaveData data)
        {
            globalMultipliers.Clear();
            resourceRetentionPercentage = 0f;
            unlockedUpgradeIDs.Clear();

            if (data == null)
            {
                Level = 0;
                Nanites = 0;
            }
            else
            {
                Level = data.Level;
                // Support legacy field if Nanites is 0 but PrestigeCurrency has value
                if (data.Nanites == 0 && data.PrestigeCurrency > 0)
                    Nanites = data.PrestigeCurrency;
                else
                    Nanites = data.Nanites;

                if (data.UnlockedUpgradeIDs != null)
                {
                    foreach (var id in data.UnlockedUpgradeIDs)
                    {
                        unlockedUpgradeIDs.Add(id);
                        ApplyUpgradeEffect(id);
                    }
                }
            }
        }

        public CivilizationSaveData GetSaveData()
        {
            return new CivilizationSaveData
            {
                Level = Level,
                Nanites = Nanites,
                UnlockedUpgradeIDs = unlockedUpgradeIDs.ToList(),
                PrestigeCurrency = Nanites // Sync legacy
            };
        }

        public void Ascend()
        {
            // Calculate gains
            Level++;
            float gainedNanites = 100f * Level; // Simple formula
            Nanites += gainedNanites;

            GameLogger.Log($"Ascended to Civilization Level {Level}! Gained {gainedNanites} Nanites.");

            // Trigger Global Reset via PersistenceManager
            PersistenceManager.Instance.ResetGameKeepCivilization();
        }

        public bool BuyUpgrade(string upgradeID)
        {
            if (unlockedUpgradeIDs.Contains(upgradeID)) return false;

            var upgrade = availableUpgrades.Find(u => u.ID == upgradeID);
            if (upgrade != null)
            {
                if (Nanites >= upgrade.Cost)
                {
                    Nanites -= upgrade.Cost;
                    unlockedUpgradeIDs.Add(upgradeID);
                    ApplyUpgradeEffect(upgradeID);
                    GameLogger.Log($"Purchased Meta Upgrade: {upgrade.Name}");
                    return true;
                }
                else
                {
                    GameLogger.Log("Not enough Nanites.");
                }
            }
            return false;
        }

        private void ApplyUpgradeEffect(string id)
        {
            var upgrade = availableUpgrades.Find(u => u.ID == id);
            if (upgrade != null && upgrade.Effect != null)
            {
                upgrade.Effect.ApplyRuntime();
            }
        }

        // --- Effect Helpers ---

        public void AddGlobalMultiplier(string statName, float amount)
        {
            if (!globalMultipliers.ContainsKey(statName)) globalMultipliers[statName] = 0f;
            globalMultipliers[statName] += amount;
        }

        public float GetGlobalMultiplier(string statName)
        {
            // Base Multiplier (Civ Level) always applies to "GlobalProduction" or similar?
            // User: "Multiplier => 1.0f + (Level * 0.1f);" was the old logic.
            // We should treat Level as a generic multiplier or specific one?
            // Let's assume Level applies to EVERYTHING (general efficiency).

            float total = BaseMultiplier;

            if (globalMultipliers.ContainsKey(statName))
            {
                total += globalMultipliers[statName]; // Additive bonus to the base multiplier
            }

            return total;
        }

        public void RegisterRetention(float percent)
        {
            resourceRetentionPercentage = Mathf.Clamp(resourceRetentionPercentage + percent, 0f, 1.0f);
        }

        public float GetRetentionPercentage()
        {
            return resourceRetentionPercentage;
        }
    }
}
