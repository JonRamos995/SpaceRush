using System;
using System.Collections.Generic;

namespace SpaceRush.Models
{
    [Serializable]
    public class GameSaveData
    {
        public long Timestamp; // For Idle calculation
        public float Credits;
        public string CurrentLocationID; // Save where the player is

        public List<ResourceSaveData> Resources = new List<ResourceSaveData>();
        public FleetSaveData Fleet;
        public ResearchSaveData Research;
        public List<LocationSaveData> Locations = new List<LocationSaveData>();
        public List<LogisticsAllocationSaveData> LogisticsAllocations = new List<LogisticsAllocationSaveData>();
        public WorkshopSaveData Workshop = new WorkshopSaveData();
        public CivilizationSaveData Civilization = new CivilizationSaveData();
    }

    [Serializable]
    public class CivilizationSaveData
    {
        public int Level;
        public float Nanites; // Replaces PrestigeCurrency concept
        public List<string> UnlockedUpgradeIDs = new List<string>();

        // Legacy support if needed, but for now we assume new saves
        public float PrestigeCurrency;
    }

    [Serializable]
    public class LogisticsAllocationSaveData
    {
        public ResourceType Type;
        public float Percentage;
    }

    [Serializable]
    public class ResourceSaveData
    {
        public ResourceType Type;
        public int Quantity;
    }

    [Serializable]
    public class FleetSaveData
    {
        public int ShipLevel;
        public float RepairStatus;
    }

    [Serializable]
    public class ResearchSaveData
    {
        public int Researchers;
        public float ResearchPoints;
        public List<string> UnlockedTechIDs = new List<string>();
    }

    [Serializable]
    public class LocationSaveData
    {
        public string ID;
        public bool IsUnlocked;
        public int State; // Cast to DiscoveryState enum

        public int MiningLevel;
        public int LogisticsLevel;
        public int StationLevel;

        public List<ResourceSaveData> InstalledMachines = new List<ResourceSaveData>();
        public List<ResourceSaveData> Stockpile = new List<ResourceSaveData>();
    }
}
