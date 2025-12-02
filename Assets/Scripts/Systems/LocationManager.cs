using System.Collections.Generic;
using UnityEngine;
using SpaceRush.Models;

namespace SpaceRush.Systems
{
    public enum DiscoveryState
    {
        Hidden,
        Discovered,   // Just found it
        Investigated, // Knows biome and resources
        ReadyToMine   // Infrastructure started
    }

    public enum BiomeType
    {
        Terrestrial, // Earth-like
        Barren,      // Moon
        Volcanic,
        Ice,
        GasGiant,
        AsteroidField
    }

    [System.Serializable]
    public class PlanetaryInfrastructure
    {
        public int MiningLevel = 0;       // Extracts resources
        public int LogisticsLevel = 0;    // Moves from surface to station
        public int StationLevel = 0;      // Storage capacity at station
    }

    [System.Serializable]
    public class Location
    {
        public string ID;
        public string Name;
        public float TravelCost; // Fuel/Credits to travel here

        // Progression
        public bool IsUnlocked; // Route known
        public DiscoveryState State = DiscoveryState.Hidden;

        // Planet Properties
        public BiomeType Biome;
        public List<ResourceType> AvailableResources;
        public string RequiredTechID; // Tech needed to mine here (e.g. "Heat Shielding")

        // Local Development
        public PlanetaryInfrastructure Infrastructure = new PlanetaryInfrastructure();
        public Dictionary<ResourceType, int> Stockpile = new Dictionary<ResourceType, int>();

        // Requirements to travel/unlock
        public bool RequiresShipOperational;
        public int MinShipLevel;
    }

    public class LocationManager : MonoBehaviour
    {
        public static LocationManager Instance { get; private set; }

        public Location CurrentLocation { get; private set; }
        public List<Location> Locations { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeLocations();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeLocations()
        {
            Locations = new List<Location>();

            // Earth - Starting Point
            Locations.Add(new Location
            {
                ID = "EARTH",
                Name = "Earth",
                TravelCost = 0,
                IsUnlocked = true,
                State = DiscoveryState.Investigated, // Or ReadyToMine if we want it active immediately
                Biome = BiomeType.Terrestrial,
                AvailableResources = new List<ResourceType>(),
                RequiresShipOperational = false,
                MinShipLevel = 0,
                RequiredTechID = null,
                Infrastructure = new PlanetaryInfrastructure { MiningLevel = 1, LogisticsLevel = 1, StationLevel = 1 }
            });

            // The Moon
            Locations.Add(new Location
            {
                ID = "MOON",
                Name = "The Moon",
                TravelCost = 100,
                IsUnlocked = false, // Must be found
                State = DiscoveryState.Hidden,
                Biome = BiomeType.Barren,
                AvailableResources = new List<ResourceType> { ResourceType.Iron },
                RequiresShipOperational = true,
                MinShipLevel = 1,
                RequiredTechID = null // Basic suits work
            });

            // Mars
            Locations.Add(new Location
            {
                ID = "MARS",
                Name = "Mars",
                TravelCost = 500,
                IsUnlocked = false,
                State = DiscoveryState.Hidden,
                Biome = BiomeType.Barren, // Cold but managed
                AvailableResources = new List<ResourceType> { ResourceType.Iron, ResourceType.Gold },
                RequiresShipOperational = true,
                MinShipLevel = 2,
                RequiredTechID = "ENV_SUIT_MK2"
            });

            // Asteroid Belt
            Locations.Add(new Location
            {
                ID = "ASTEROID_BELT",
                Name = "Asteroid Belt",
                TravelCost = 2000,
                IsUnlocked = false,
                State = DiscoveryState.Hidden,
                Biome = BiomeType.AsteroidField,
                AvailableResources = new List<ResourceType> { ResourceType.Iron, ResourceType.Gold, ResourceType.Platinum },
                RequiresShipOperational = true,
                MinShipLevel = 3,
                RequiredTechID = "MICRO_G_MINING"
            });

            CurrentLocation = Locations[0]; // Start at Earth
        }

        public void DiscoverLocation(string id)
        {
            var loc = Locations.Find(l => l.ID == id);
            if (loc != null && loc.State == DiscoveryState.Hidden)
            {
                loc.State = DiscoveryState.Discovered;
                loc.IsUnlocked = true;
                GameLogger.Log($"Discovered new location: {loc.Name}");
            }
        }

        public void TryTravel(string locationID)
        {
            Location target = Locations.Find(l => l.ID == locationID);
            if (target == null) return;

            // Check Ship Status
            if (target.RequiresShipOperational && !FleetManager.Instance.IsOperational)
            {
                 GameLogger.Log("Cannot travel: Ship is damaged.");
                 return;
            }

            // Check Tech Requirements (Propulsion or Biome suits)
            if (!string.IsNullOrEmpty(target.RequiredTechID))
            {
                if (!ResearchManager.Instance.IsTechUnlocked(target.RequiredTechID))
                {
                    GameLogger.Log($"Cannot travel: Requires technology.");
                    return;
                }
            }

            // Travel Cost
            if (ResourceManager.Instance.SpendCredits(target.TravelCost))
            {
                GameLogger.Log($"Traveling to {target.Name} (-{target.TravelCost} CR)...");
                CurrentLocation = target;
                if (!target.IsUnlocked) target.IsUnlocked = true;
                if (target.State == DiscoveryState.Hidden) target.State = DiscoveryState.Discovered;
            }
            else
            {
                 GameLogger.Log($"Not enough credits to travel to {target.Name}. Need {target.TravelCost}.");
            }
        }

        public void SetLocation(string id)
        {
            var loc = Locations.Find(l => l.ID == id);
            if (loc != null)
            {
                CurrentLocation = loc;
                GameLogger.Log($"Location restored: {loc.Name}");
            }
        }

        public void LoadData(List<LocationSaveData> data)
        {
            if (data == null) return;

            foreach (var locData in data)
            {
                var loc = Locations.Find(l => l.ID == locData.ID);
                if (loc != null)
                {
                    loc.IsUnlocked = locData.IsUnlocked;
                    loc.State = (DiscoveryState)locData.State;
                    loc.Infrastructure.MiningLevel = locData.MiningLevel;
                    loc.Infrastructure.LogisticsLevel = locData.LogisticsLevel;
                    loc.Infrastructure.StationLevel = locData.StationLevel;

                    loc.Stockpile.Clear();
                    foreach (var res in locData.Stockpile)
                    {
                        loc.Stockpile[res.Type] = res.Quantity;
                    }
                }
            }

            // Re-set CurrentLocation if needed, or just default to Earth or find where ship is?
            // For now, let's keep it at Earth (default) unless we want to save ship location too.
            // I'll assume ship is at Earth or we can default to the first unlocked location if Earth is somehow locked (unlikely).
            // Better yet, we should probably save "CurrentLocationID" in FleetSaveData or GlobalSaveData.
            // But for now, Earth is safe.
        }
    }
}
