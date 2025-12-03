using System.Collections.Generic;
using UnityEngine;
using SpaceRush.Core;
using SpaceRush.Models;
using SpaceRush.Data;
using System.Linq;

namespace SpaceRush.Systems
{
    public class ResearchManager : MonoBehaviour
    {
        public static ResearchManager Instance { get; private set; }

        public int Researchers { get; private set; } = 0;
        public float ResearchPoints { get; private set; } = 0f;
        // CivilizationLevel moved to CivilizationManager, kept here for legacy reads if necessary
        // but we should defer to CivilizationManager for the multiplier.

        private List<TechState> technologies;
        private float researchSpeedPerResearcher = 1.0f;
        private float researcherCost = 1000f;

        // New: Command Pattern Storage
        private Dictionary<string, float> statModifiers = new Dictionary<string, float>();
        private HashSet<string> unlockedFeatures = new HashSet<string>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
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

        private void Start()
        {
            InitializeTechTree();
        }

        private void InitializeTechTree()
        {
            technologies = new List<TechState>();
            statModifiers.Clear();
            unlockedFeatures.Clear();

            if (GameDatabase.Instance == null) return;

            foreach (var def in GameDatabase.Instance.Technologies)
            {
                // Inject Effects (Simulating Editor setup)
                AssignEffectsToTech(def);
                technologies.Add(new TechState(def));
            }
        }

        private void AssignEffectsToTech(TechDefinition def)
        {
            // Ideally, this is done in the Editor. Here we map legacy IDs to Effects.
            if (def.Effects == null) def.Effects = new List<TechEffect>();

            // Avoid duplicate additions if run multiple times
            if (def.Effects.Count > 0) return;

            switch (def.ID)
            {
                case "TERRAFORMING_BASICS":
                    // This used to raise CivLevel, but CivLevel is now in CivilizationManager.
                    // We can create a generic "ActionEffect" or handle it.
                    // For now, let's skip or reimplement if needed.
                    break;
                case "EFFICIENCY_1":
                    var eff = ScriptableObject.CreateInstance<StatBonusEffect>();
                    eff.StatName = "MiningSpeed";
                    eff.Modifier = 0.1f;
                    def.Effects.Add(eff);
                    break;
                case "REPAIR_DROID": // New Tech
                    var rep = ScriptableObject.CreateInstance<UnlockFeatureEffect>();
                    rep.FeatureID = "REPAIR_DROID";
                    def.Effects.Add(rep);
                    break;
                // Add other tech mappings here
            }
        }

        private void Update()
        {
            if (Researchers > 0)
            {
                float gained = Researchers * researchSpeedPerResearcher * Time.deltaTime;
                ResearchPoints += gained;
            }
        }

        public void HireResearcher()
        {
            if (ResourceManager.Instance.SpendCredits(researcherCost))
            {
                Researchers++;
                GameLogger.Log($"Hired Researcher. Total: {Researchers}");
            }
        }

        public void InvestInResearch(float creditAmount)
        {
            if (ResourceManager.Instance.SpendCredits(creditAmount))
            {
                float pointsGained = creditAmount * 0.1f;
                ResearchPoints += pointsGained;
                GameLogger.Log($"Invested {creditAmount} credits for {pointsGained} RP.");
            }
        }

        public void ResetData()
        {
            Researchers = 0;
            ResearchPoints = 0;
            statModifiers.Clear();
            unlockedFeatures.Clear();

            if (technologies != null)
            {
                foreach (var tech in technologies)
                {
                    tech.IsUnlocked = false;
                }
            }
        }

        public void UnlockTechnology(string techID)
        {
            TechState tech = technologies.Find(t => t.ID == techID);
            if (tech != null && !tech.IsUnlocked)
            {
                 if (tech.Definition == null) tech.Definition = GameDatabase.Instance.GetTech(techID);

                 if (ResearchPoints >= tech.Definition.ResearchPointsRequired)
                 {
                    ResearchPoints -= tech.Definition.ResearchPointsRequired;
                    tech.IsUnlocked = true;
                    ApplyTechEffects(tech); // New Logic
                    GameLogger.Log($"Unlocked {tech.Definition.Name}!");
                 }
            }
        }

        private void ApplyTechEffects(TechState tech)
        {
            if (tech.Definition.Effects != null)
            {
                foreach (var effect in tech.Definition.Effects)
                {
                    effect.Apply();
                }
            }
        }

        // --- Command Pattern Helpers ---

        public void AddStatBonus(string statName, float amount)
        {
            if (!statModifiers.ContainsKey(statName)) statModifiers[statName] = 0f;
            statModifiers[statName] += amount;

            // Trigger recalculations if needed
            if (statName == "MiningSpeed" || statName == "GlobalProduction")
            {
                if (FleetManager.Instance != null) FleetManager.Instance.RecalculateStats();
            }
        }

        public float GetStatBonus(string statName)
        {
            if (statModifiers.ContainsKey(statName)) return statModifiers[statName];
            return 0f;
        }

        public void UnlockFeature(string featureID)
        {
            if (!unlockedFeatures.Contains(featureID))
            {
                unlockedFeatures.Add(featureID);
                GameLogger.Log($"Feature Unlocked: {featureID}");
            }
        }

        public bool IsFeatureUnlocked(string featureID)
        {
            return unlockedFeatures.Contains(featureID);
        }

        // --- Legacy/Support ---

        public bool IsTechUnlocked(string techID)
        {
            if (string.IsNullOrEmpty(techID)) return true;
            if (technologies == null) InitializeTechTree();
            TechState tech = technologies.Find(t => t.ID == techID);
            return tech != null && tech.IsUnlocked;
        }

        public List<string> GetUnlockedTechIDs()
        {
            if (technologies == null) InitializeTechTree();
            return technologies.Where(t => t.IsUnlocked).Select(t => t.ID).ToList();
        }

        public List<TechState> GetAllTechs()
        {
            if (technologies == null) InitializeTechTree();
            return technologies;
        }

        public void LoadData(ResearchSaveData data)
        {
            if (data == null) return;
            Researchers = data.Researchers;
            ResearchPoints = data.ResearchPoints;

            // Ensure initialized
            if (technologies == null || technologies.Count == 0) InitializeTechTree();

            // Clear previous effects to avoid doubling up on load
            statModifiers.Clear();
            unlockedFeatures.Clear();

            if (data.UnlockedTechIDs != null)
            {
                foreach (var id in data.UnlockedTechIDs)
                {
                    TechState tech = technologies.Find(t => t.ID == id);
                    if (tech != null)
                    {
                        tech.IsUnlocked = true;
                        if (tech.Definition == null) tech.Definition = GameDatabase.Instance.GetTech(id);

                        // Re-apply effects on load
                        // Note: AssignEffectsToTech is called in InitializeTechTree, so Effects list is populated.
                        ApplyTechEffects(tech);
                    }
                }
            }
        }
    }
}
