using UnityEngine;
using SpaceRush.Core;
using SpaceRush.Models;
using System.Collections;
using SpaceRush.Data; // Needed for LocationDefinition properties access if used

namespace SpaceRush.Systems
{
    public class PlanetarySystem : MonoBehaviour
    {
        public static PlanetarySystem Instance { get; private set; }

        private const float INVESTIGATION_COST = 500f;
        private const float INVESTIGATION_TIME = 5.0f; // Seconds

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
            StartCoroutine(PlanetaryProductionLoop());
        }

        private IEnumerator PlanetaryProductionLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(1.0f); // Tick every second

                if (LocationManager.Instance != null && LocationManager.Instance.Locations != null)
                {
                    foreach (var loc in LocationManager.Instance.Locations)
                    {
                        if (loc.State == DiscoveryState.ReadyToMine && loc.Infrastructure.MiningLevel > 0)
                        {
                            ProduceResources(loc);
                        }
                    }
                }
            }
        }

        public void ProduceResources(LocationState loc)
        {
            // Production formula: MiningLevel * BiomeMultiplier (1.0 for now)
            int productionAmount = loc.Infrastructure.MiningLevel;

            // Check Station Capacity
            // Simple capacity logic: StationLevel * 100
            int capacity = loc.Infrastructure.StationLevel * 100;
            int currentStock = 0;
            foreach(var kvp in loc.Stockpile) currentStock += kvp.Value;

            if (currentStock >= capacity && capacity > 0)
            {
                // Storage Full
                return;
            }

            // Produce random available resource from Definition
            if (loc.Definition.AvailableResources.Count > 0)
            {
                var resType = loc.Definition.AvailableResources[UnityEngine.Random.Range(0, loc.Definition.AvailableResources.Count)];

                if (!loc.Stockpile.ContainsKey(resType))
                    loc.Stockpile[resType] = 0;

                loc.Stockpile[resType] += productionAmount;
            }
        }

        // --- Interaction Methods ---

        public void InvestigatePlanet(string locationID)
        {
            LocationState loc = LocationManager.Instance.Locations.Find(l => l.ID == locationID);
            if (loc == null) return;

            if (loc.State == DiscoveryState.Discovered)
            {
                if (ResourceManager.Instance.SpendCredits(INVESTIGATION_COST))
                {
                    StartCoroutine(InvestigateRoutine(loc));
                }
            }
        }

        private IEnumerator InvestigateRoutine(LocationState loc)
        {
            GameLogger.Log($"Investigating {loc.Definition.Name}...");
            yield return new WaitForSeconds(INVESTIGATION_TIME);

            loc.State = DiscoveryState.Investigated;
            GameLogger.Log($"Investigation Complete! {loc.Definition.Name} Biome: {loc.Definition.Biome}. Required Tech: {loc.Definition.RequiredTechID}");
        }

        public void StartMiningOperations(string locationID)
        {
            LocationState loc = LocationManager.Instance.Locations.Find(l => l.ID == locationID);
            if (loc == null || loc.State != DiscoveryState.Investigated) return;

            // Check Tech Requirements
            if (!ResearchManager.Instance.IsTechUnlocked(loc.Definition.RequiredTechID))
            {
                GameLogger.Log($"Cannot start mining: Missing Tech {loc.Definition.RequiredTechID}");
                return;
            }

            // Initial Infrastructure Cost
            float startupCost = 1000f;
            if (ResourceManager.Instance.SpendCredits(startupCost))
            {
                loc.State = DiscoveryState.ReadyToMine;
                loc.Infrastructure.MiningLevel = 1;
                loc.Infrastructure.StationLevel = 1;
                loc.Infrastructure.LogisticsLevel = 1;
                GameLogger.Log($"Mining Operations established on {loc.Definition.Name}");
            }
        }

        public void UpgradeInfrastructure(string locationID, string type)
        {
            LocationState loc = LocationManager.Instance.Locations.Find(l => l.ID == locationID);
            if (loc == null || loc.State != DiscoveryState.ReadyToMine) return;

            float cost = 0f;

            // Simple cost scaling
            if (type == "Mining") cost = loc.Infrastructure.MiningLevel * 500f;
            else if (type == "Station") cost = loc.Infrastructure.StationLevel * 500f;
            else if (type == "Logistics") cost = loc.Infrastructure.LogisticsLevel * 500f;

            if (ResourceManager.Instance.SpendCredits(cost))
            {
                if (type == "Mining") loc.Infrastructure.MiningLevel++;
                else if (type == "Station") loc.Infrastructure.StationLevel++;
                else if (type == "Logistics") loc.Infrastructure.LogisticsLevel++;

                GameLogger.Log($"Upgraded {type} on {loc.Definition.Name}.");
            }
        }
    }
}
