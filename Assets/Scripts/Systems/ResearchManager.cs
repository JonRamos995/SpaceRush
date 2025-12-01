using System.Collections.Generic;
using UnityEngine;
using SpaceRush.Core;

namespace SpaceRush.Systems
{
    [System.Serializable]
    public class Technology
    {
        public string ID;
        public string Name;
        public string Description;
        public float Cost;
        public bool IsUnlocked;
        public int ResearchPointsRequired;
    }

    public class ResearchManager : MonoBehaviour
    {
        public static ResearchManager Instance { get; private set; }

        public int Researchers { get; private set; } = 0;
        public float ResearchPoints { get; private set; } = 0f;
        public float CivilizationLevel { get; private set; } = 1.0f; // Multiplier for global benefits

        private List<Technology> technologies;
        private float researchSpeedPerResearcher = 1.0f;
        private float researcherCost = 1000f; // Cost to hire one

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeTechTree();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeTechTree()
        {
            technologies = new List<Technology>
            {
                // Basic Upgrades
                new Technology { ID = "EFFICIENCY_1", Name = "Mining Efficiency I", Description = "Improves mining speed by 10%", Cost = 500, ResearchPointsRequired = 100, IsUnlocked = false },
                new Technology { ID = "MARKET_ANALYSIS", Name = "Market Analysis AI", Description = "Better trade prices", Cost = 1000, ResearchPointsRequired = 250, IsUnlocked = false },
                new Technology { ID = "ADV_PROPULSION", Name = "Advanced Propulsion", Description = "Unlocks distant planets", Cost = 5000, ResearchPointsRequired = 1000, IsUnlocked = false },
                new Technology { ID = "TERRAFORMING_BASICS", Name = "Terraforming Basics", Description = "Increases Civilization Level", Cost = 10000, ResearchPointsRequired = 2500, IsUnlocked = false },

                // Planetary Mining Techs
                new Technology { ID = "ENV_SUIT_MK2", Name = "Environmental Suits Mk2", Description = "Allows mining on Mars-like planets", Cost = 2000, ResearchPointsRequired = 500, IsUnlocked = false },
                new Technology { ID = "MICRO_G_MINING", Name = "Micro-G Anchors", Description = "Allows mining in Asteroid Belts", Cost = 3000, ResearchPointsRequired = 800, IsUnlocked = false },
                new Technology { ID = "THERMAL_SHIELDING", Name = "Thermal Shielding", Description = "Allows mining on Volcanic planets", Cost = 8000, ResearchPointsRequired = 2000, IsUnlocked = false },

                // Infrastructure Techs
                new Technology { ID = "AUTO_LOGISTICS", Name = "Automated Logistics", Description = "Unlock automated trade routes", Cost = 5000, ResearchPointsRequired = 1500, IsUnlocked = false }
            };
        }

        private void Update()
        {
            // Passive research generation
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
                Debug.Log($"Hired Researcher. Total: {Researchers}");
            }
        }

        public void InvestInResearch(float creditAmount)
        {
            if (ResourceManager.Instance.SpendCredits(creditAmount))
            {
                // Converting money directly to speed up current projects or buy raw points
                float pointsGained = creditAmount * 0.1f; // 10 Credits = 1 RP
                ResearchPoints += pointsGained;
                Debug.Log($"Invested {creditAmount} credits for {pointsGained} RP.");
            }
        }

        public void UnlockTechnology(string techID)
        {
            Technology tech = technologies.Find(t => t.ID == techID);
            if (tech != null && !tech.IsUnlocked && ResearchPoints >= tech.ResearchPointsRequired)
            {
                ResearchPoints -= tech.ResearchPointsRequired;
                tech.IsUnlocked = true;
                ApplyTechEffect(tech);
                Debug.Log($"Unlocked {tech.Name}!");
            }
        }

        public bool IsTechUnlocked(string techID)
        {
            if (string.IsNullOrEmpty(techID)) return true; // No requirement
            Technology tech = technologies.Find(t => t.ID == techID);
            return tech != null && tech.IsUnlocked;
        }

        private void ApplyTechEffect(Technology tech)
        {
            switch (tech.ID)
            {
                case "TERRAFORMING_BASICS":
                    CivilizationLevel += 0.5f;
                    Debug.Log("Civilization Level Increased!");
                    break;
                // Other effects would be hooked into other managers
            }
        }
    }
}
