using System.Collections.Generic;
using UnityEngine;
using SpaceRush.Models;
using SpaceRush.Core;
using SpaceRush.Data;

namespace SpaceRush.Systems
{
    public class LocationManager : MonoBehaviour
    {
        public static LocationManager Instance { get; private set; }

        public LocationState CurrentLocation { get; private set; }
        public List<LocationState> Locations { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                // We attempt initialization here, but GameDatabase must be ready.
                // If GameDatabase is on the same GameObject or initialized before, this works.
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
            // Optional re-check if not initialized in Awake
            if (Locations == null || Locations.Count == 0) InitializeLocations();
        }

        private void InitializeLocations()
        {
            Locations = new List<LocationState>();

            // Load definitions from Database
            if (GameDatabase.Instance == null)
            {
                // If GameDatabase initializes in Awake, it should be ready if execution order is right.
                // If not, we might need to wait.
                // However, since we are in Awake/Start, let's try to access it.
                // If GameDatabase is also a singleton, we need to ensure it's ready.
                // Ideally GameDatabase is not a MonoBehaviour but a pure C# class or initializes very early.
                // Here we assume GameDatabase.Instance is set.
                if (GameDatabase.Instance == null)
                {
                     // Fallback or error
                     Debug.LogError("GameDatabase not initialized!");
                     return;
                }
            }

            foreach (var def in GameDatabase.Instance.Locations)
            {
                var state = new LocationState(def);

                // Initialize default state for Earth
                if (def.ID == "EARTH")
                {
                    state.IsUnlocked = true;
                    state.State = DiscoveryState.Investigated;
                    state.Infrastructure.MiningLevel = 1;
                    state.Infrastructure.LogisticsLevel = 1;
                    state.Infrastructure.StationLevel = 1;
                }

                Locations.Add(state);
            }

            if (Locations.Count > 0)
                CurrentLocation = Locations[0]; // Default to Earth
        }

        public void DiscoverLocation(string id)
        {
            var loc = Locations.Find(l => l.ID == id);
            if (loc != null && loc.State == DiscoveryState.Hidden)
            {
                loc.State = DiscoveryState.Discovered;
                loc.IsUnlocked = true;
                GameLogger.Log($"Discovered new location: {loc.Definition.Name}");
            }
        }

        public void TryTravel(string locationID)
        {
            LocationState target = Locations.Find(l => l.ID == locationID);
            if (target == null) return;

            // Check Ship Status
            if (target.Definition.RequiresShipOperational && !FleetManager.Instance.IsOperational)
            {
                 GameLogger.Log("Cannot travel: Ship is damaged.");
                 return;
            }

            // Check Tech Requirements
            if (!string.IsNullOrEmpty(target.Definition.RequiredTechID))
            {
                if (!ResearchManager.Instance.IsTechUnlocked(target.Definition.RequiredTechID))
                {
                    GameLogger.Log($"Cannot travel: Requires technology.");
                    return;
                }
            }

            // Travel Cost
            if (ResourceManager.Instance.SpendCredits(target.Definition.TravelCost))
            {
                GameLogger.Log($"Traveling to {target.Definition.Name} (-{target.Definition.TravelCost} CR)...");
                CurrentLocation = target;
                if (!target.IsUnlocked) target.IsUnlocked = true;
                if (target.State == DiscoveryState.Hidden) target.State = DiscoveryState.Discovered;
            }
            else
            {
                 GameLogger.Log($"Not enough credits to travel to {target.Definition.Name}. Need {target.Definition.TravelCost}.");
            }
        }

        public void SetLocation(string id)
        {
            var loc = Locations.Find(l => l.ID == id);
            if (loc != null)
            {
                CurrentLocation = loc;
                GameLogger.Log($"Location restored: {loc.Definition.Name}");
            }
        }

        public void LoadData(List<LocationSaveData> data)
        {
            if (data == null) return;

            // Ensure we are initialized first (if Load comes after Start, which it usually does in GameManager)
            if (Locations == null || Locations.Count == 0) InitializeLocations();

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
        }
    }
}
