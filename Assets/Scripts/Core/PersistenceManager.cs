using UnityEngine;
using System.IO;
using SpaceRush.Models;
using SpaceRush.Systems;
using System;
using System.Collections.Generic;

namespace SpaceRush.Core
{
    public class PersistenceManager : MonoBehaviour
    {
        public static PersistenceManager Instance { get; private set; }

        private string saveFilePath;
        private const string SAVE_FILE_NAME = "savegame.json";

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                saveFilePath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
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

        public void SaveGame()
        {
            GameSaveData data = new GameSaveData();

            // 1. Global / Resource Data
            data.Credits = ResourceManager.Instance.Credits;
            data.CurrentLocationID = LocationManager.Instance.CurrentLocation?.ID;
            data.Timestamp = DateTime.UtcNow.ToBinary();

            foreach (var res in ResourceManager.Instance.GetAllResources())
            {
                data.Resources.Add(new ResourceSaveData
                {
                    Type = res.Type,
                    Quantity = res.Quantity
                });
            }

            // 2. Fleet Data
            data.Fleet = new FleetSaveData
            {
                ShipLevel = FleetManager.Instance.ShipLevel,
                RepairStatus = FleetManager.Instance.RepairStatus
            };

            // Workshop Data
            if (WorkshopManager.Instance != null)
            {
                data.Workshop = WorkshopManager.Instance.GetSaveData();
            }

            // 3. Research Data
            data.Research = new ResearchSaveData
            {
                Researchers = ResearchManager.Instance.Researchers,
                ResearchPoints = ResearchManager.Instance.ResearchPoints
            };
            // We need to access unlocked techs. Since the list is private in ResearchManager,
            // we will need to expose a way to get them, or iterate known IDs.
            // For now, I will add a method to ResearchManager to get unlocked IDs.
            data.Research.UnlockedTechIDs = ResearchManager.Instance.GetUnlockedTechIDs();


            // 4. Location Data
            foreach (var loc in LocationManager.Instance.Locations)
            {
                LocationSaveData locData = new LocationSaveData
                {
                    ID = loc.ID,
                    IsUnlocked = loc.IsUnlocked,
                    State = (int)loc.State,
                    MiningLevel = loc.Infrastructure.MiningLevel,
                    LogisticsLevel = loc.Infrastructure.LogisticsLevel,
                    StationLevel = loc.Infrastructure.StationLevel
                };

                if (loc.Infrastructure.InstalledMachines != null)
                {
                    foreach (var kvp in loc.Infrastructure.InstalledMachines)
                    {
                        locData.InstalledMachines.Add(new ResourceSaveData { Type = kvp.Key, Quantity = kvp.Value });
                    }
                }

                foreach (var kvp in loc.Stockpile)
                {
                    locData.Stockpile.Add(new ResourceSaveData { Type = kvp.Key, Quantity = kvp.Value });
                }

                data.Locations.Add(locData);
            }

            // 5. Logistics Allocations
            if (LogisticsSystem.Instance != null && LogisticsSystem.Instance.CargoAllocations != null)
            {
                foreach (var kvp in LogisticsSystem.Instance.CargoAllocations)
                {
                    data.LogisticsAllocations.Add(new LogisticsAllocationSaveData
                    {
                        Type = kvp.Key,
                        Percentage = kvp.Value
                    });
                }
            }

            // Write to file
            try
            {
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(saveFilePath, json);
                GameLogger.Log("Game Saved.");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save game: {e.Message}");
            }
        }

        public bool LoadGame()
        {
            if (!File.Exists(saveFilePath))
            {
                GameLogger.Log("No save file found. Starting new game.");
                return false;
            }

            try
            {
                string json = File.ReadAllText(saveFilePath);
                GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);

                if (data == null)
                {
                    Debug.LogError("Failed to parse save data.");
                    return false;
                }

                ApplyLoadData(data);
                GameLogger.Log("Game Loaded Successfully.");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load game: {e.Message}");
                return false;
            }
        }

        private void ApplyLoadData(GameSaveData data)
        {
            // 1. Resources
            ResourceManager.Instance.SetCredits(data.Credits);
            foreach (var rData in data.Resources)
            {
                ResourceManager.Instance.SetResource(rData.Type, rData.Quantity);
            }

            // 2. Research (Load before Fleet to ensure Tech Bonuses are known)
            ResearchManager.Instance.LoadData(data.Research);

            // 3. Fleet
            FleetManager.Instance.LoadData(data.Fleet);

            // Workshop
            if (WorkshopManager.Instance != null)
            {
                WorkshopManager.Instance.LoadData(data.Workshop);
            }

            // 4. Locations
            LocationManager.Instance.LoadData(data.Locations);

            // Restore Installed Machines (LoadData only does basic props)
            foreach (var locData in data.Locations)
            {
                var loc = LocationManager.Instance.Locations.Find(l => l.ID == locData.ID);
                if (loc != null && locData.InstalledMachines != null)
                {
                    loc.Infrastructure.InstalledMachines.Clear();
                    foreach (var m in locData.InstalledMachines)
                    {
                        loc.Infrastructure.InstalledMachines[m.Type] = m.Quantity;
                    }
                }
            }

            // Restore Current Location
            if (!string.IsNullOrEmpty(data.CurrentLocationID))
            {
                LocationManager.Instance.SetLocation(data.CurrentLocationID);
            }

            // 5. Logistics Allocations
            if (LogisticsSystem.Instance != null)
            {
                // Clear existing
                LogisticsSystem.Instance.CargoAllocations.Clear();
                if (data.LogisticsAllocations != null)
                {
                    foreach (var alloc in data.LogisticsAllocations)
                    {
                        LogisticsSystem.Instance.SetAllocation(alloc.Type, alloc.Percentage);
                    }
                }
            }

            // 6. Idle
            IdleManager.Instance.CalculateOfflineProgressFromTimestamp(data.Timestamp);
        }
    }
}
