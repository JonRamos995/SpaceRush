using UnityEngine;

namespace SpaceRush.Models
{
    [System.Serializable]
    public enum ResourceType
    {
        Iron,
        Gold,
        Platinum,
        Diamond,
        Antimatter,
        Steel,
        Circuit,
        Hydrogen,
        // New Intermediate
        IronIngot,
        GoldIngot,
        Plating,
        // New Machines (Installable)
        MiningDrill,
        AutoMiner,
        LogisticsBot,
        // Expansion 1
        Titanium,
        Helium3,
        CryoFluid,
        Ice
    }

    [System.Serializable]
    public class ResourceData
    {
        public ResourceType Type;
        public string Name;
        public float BaseValue;
        public float CurrentMarketValue;
        public int Quantity;

        public ResourceData(ResourceType type, string name, float baseValue)
        {
            Type = type;
            Name = name;
            BaseValue = baseValue;
            CurrentMarketValue = baseValue;
            Quantity = 0;
        }
    }
}
