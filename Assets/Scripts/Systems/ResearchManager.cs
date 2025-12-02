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
        public float CivilizationLevel { get; private set; } = 1.0f;

        private List<TechState> technologies;
        private float researchSpeedPerResearcher = 1.0f;
        private float researcherCost = 1000f;

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

            if (GameDatabase.Instance == null) return;

            foreach (var def in GameDatabase.Instance.Technologies)
            {
                technologies.Add(new TechState(def));
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
            // CivilizationLevel is now managed by CivilizationManager, but we keep this for compatibility if needed
            CivilizationLevel = 1.0f;

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
                 // Ensure Definition is linked (if serialization broke it, though in memory it should be fine)
                 if (tech.Definition == null) tech.Definition = GameDatabase.Instance.GetTech(techID);

                 if (ResearchPoints >= tech.Definition.ResearchPointsRequired)
                 {
                    ResearchPoints -= tech.Definition.ResearchPointsRequired;
                    tech.IsUnlocked = true;
                    ApplyTechEffect(tech);
                    GameLogger.Log($"Unlocked {tech.Definition.Name}!");
                 }
            }
        }

        public bool IsTechUnlocked(string techID)
        {
            if (string.IsNullOrEmpty(techID)) return true;
            if (technologies == null) InitializeTechTree();
            TechState tech = technologies.Find(t => t.ID == techID);
            return tech != null && tech.IsUnlocked;
        }

        public List<string> GetUnlockedTechIDs()
        {
            return technologies.Where(t => t.IsUnlocked).Select(t => t.ID).ToList();
        }

        public List<TechState> GetAllTechnologies()
        {
            return technologies;
        }

        public void LoadData(ResearchSaveData data)
        {
            if (data == null) return;
            Researchers = data.Researchers;
            ResearchPoints = data.ResearchPoints;
            CivilizationLevel = 1.0f;

            // Ensure initialized
            if (technologies == null || technologies.Count == 0) InitializeTechTree();

            if (data.UnlockedTechIDs != null)
            {
                foreach (var id in data.UnlockedTechIDs)
                {
                    TechState tech = technologies.Find(t => t.ID == id);
                    if (tech != null)
                    {
                        tech.IsUnlocked = true;
                        if (tech.Definition == null) tech.Definition = GameDatabase.Instance.GetTech(id);
                        ApplyTechEffect(tech);
                    }
                }
            }
        }

        private void ApplyTechEffect(TechState tech)
        {
            switch (tech.ID)
            {
                case "TERRAFORMING_BASICS":
                    CivilizationLevel += 0.5f;
                    GameLogger.Log("Civilization Level Increased!");
                    break;
                case "EFFICIENCY_1":
                    FleetManager.Instance.RecalculateStats();
                    break;
            }
        }
    }
}
