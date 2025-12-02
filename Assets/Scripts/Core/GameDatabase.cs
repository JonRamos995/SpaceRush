using UnityEngine;
using SpaceRush.Data;
using SpaceRush.Systems; // For BiomeType
using SpaceRush.Models;
using System.Collections.Generic;

namespace SpaceRush.Core
{
    /// <summary>
    /// Acts as a central repository for all static game data.
    /// In a full production pipeline, this would load Assets from Resources/Addressables.
    /// For this prototype, we populate it manually in code to avoid Editor dependency issues.
    /// </summary>
    public class GameDatabase : MonoBehaviour
    {
        public static GameDatabase Instance { get; private set; }

        public List<LocationDefinition> Locations { get; private set; }
        public List<TechDefinition> Technologies { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeDatabase();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeDatabase()
        {
            Locations = new List<LocationDefinition>();
            Technologies = new List<TechDefinition>();

            // --- Populate Technologies ---
            Technologies.Add(new TechDefinition("EFFICIENCY_1", "Mining Efficiency I", "Improves mining speed by 10%", 500, 100));
            Technologies.Add(new TechDefinition("MARKET_ANALYSIS", "Market Analysis AI", "Better trade prices", 1000, 250));
            Technologies.Add(new TechDefinition("ADV_PROPULSION", "Advanced Propulsion", "Unlocks distant planets", 5000, 1000));
            Technologies.Add(new TechDefinition("TERRAFORMING_BASICS", "Terraforming Basics", "Increases Civilization Level", 10000, 2500));
            Technologies.Add(new TechDefinition("ENV_SUIT_MK2", "Environmental Suits Mk2", "Allows mining on Mars-like planets", 2000, 500));
            Technologies.Add(new TechDefinition("MICRO_G_MINING", "Micro-G Anchors", "Allows mining in Asteroid Belts", 3000, 800));
            Technologies.Add(new TechDefinition("THERMAL_SHIELDING", "Thermal Shielding", "Allows mining on Volcanic planets", 8000, 2000));
            Technologies.Add(new TechDefinition("AUTO_LOGISTICS", "Automated Logistics", "Unlock automated trade routes", 5000, 1500));


            // --- Populate Locations ---
            Locations.Add(new LocationDefinition(
                "EARTH", "Earth", 0, BiomeType.Terrestrial,
                new List<ResourceType>(),
                null, 0
            ));

            Locations.Add(new LocationDefinition(
                "MOON", "The Moon", 100, BiomeType.Barren,
                new List<ResourceType> { ResourceType.Iron },
                null, 1
            ));

            Locations.Add(new LocationDefinition(
                "MARS", "Mars", 500, BiomeType.Barren,
                new List<ResourceType> { ResourceType.Iron, ResourceType.Gold },
                "ENV_SUIT_MK2", 2
            ));

            Locations.Add(new LocationDefinition(
                "ASTEROID_BELT", "Asteroid Belt", 2000, BiomeType.AsteroidField,
                new List<ResourceType> { ResourceType.Iron, ResourceType.Gold, ResourceType.Platinum },
                "MICRO_G_MINING", 3
            ));
        }

        public LocationDefinition GetLocation(string id)
        {
            return Locations.Find(l => l.ID == id);
        }

        public TechDefinition GetTech(string id)
        {
            return Technologies.Find(t => t.ID == id);
        }
    }
}
