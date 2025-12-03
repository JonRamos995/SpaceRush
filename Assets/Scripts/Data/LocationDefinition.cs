using UnityEngine;
using SpaceRush.Models;
using SpaceRush.Systems; // For BiomeType
using System.Collections.Generic;

namespace SpaceRush.Data
{
    [System.Serializable]
    public class LocationDefinition
    {
        public string ID;
        public string Name;
        public string Description;
        public float TravelCost;
        public BiomeType Biome;
        public List<ResourceType> AvailableResources;
        public string RequiredTechID;
        public int MinShipLevel;
        public bool RequiresShipOperational;

        // Synergy / Consumption Boost
        public bool HasSynergy;
        public ResourceType SynergyResource;
        public float SynergyMultiplier; // e.g. 1.0 for +100%

        public LocationDefinition(string id, string name, float cost, BiomeType biome, List<ResourceType> resources, string techReq = null, int minShip = 0)
        {
            ID = id;
            Name = name;
            TravelCost = cost;
            Biome = biome;
            AvailableResources = resources;
            RequiredTechID = techReq;
            MinShipLevel = minShip;
            RequiresShipOperational = true;
        }

        // Default constructor for ScriptableObject creation in Editor
        public LocationDefinition() {}
    }
}
