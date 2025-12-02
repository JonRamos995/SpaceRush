using UnityEngine;
using System.Collections.Generic;
using SpaceRush.Models;

namespace SpaceRush.Core
{
    public class ResourceManager : MonoBehaviour
    {
        public static ResourceManager Instance { get; private set; }

        public float Credits { get; private set; }
        private Dictionary<ResourceType, ResourceData> resources;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeResources();
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

        private void InitializeResources()
        {
            resources = new Dictionary<ResourceType, ResourceData>();

            // Define basic resources
            AddResourceDefinition(new ResourceData(ResourceType.Iron, "Iron", 10f));
            AddResourceDefinition(new ResourceData(ResourceType.Gold, "Gold", 50f));
            AddResourceDefinition(new ResourceData(ResourceType.Platinum, "Platinum", 150f));
            AddResourceDefinition(new ResourceData(ResourceType.Diamond, "Diamond", 500f));
            AddResourceDefinition(new ResourceData(ResourceType.Antimatter, "Antimatter", 2500f));
            AddResourceDefinition(new ResourceData(ResourceType.Steel, "Steel", 60f));
            AddResourceDefinition(new ResourceData(ResourceType.Circuit, "Circuit", 120f));
            AddResourceDefinition(new ResourceData(ResourceType.Hydrogen, "Hydrogen", 25f));

            // New Resources
            AddResourceDefinition(new ResourceData(ResourceType.IronIngot, "Iron Ingot", 20f));
            AddResourceDefinition(new ResourceData(ResourceType.GoldIngot, "Gold Ingot", 100f));
            AddResourceDefinition(new ResourceData(ResourceType.Plating, "Plating", 50f));
            AddResourceDefinition(new ResourceData(ResourceType.MiningDrill, "Mining Drill", 500f));
            AddResourceDefinition(new ResourceData(ResourceType.AutoMiner, "Auto Miner", 2000f));
            AddResourceDefinition(new ResourceData(ResourceType.LogisticsBot, "Logistics Bot", 1000f));

            Credits = 0f;
        }

        private void AddResourceDefinition(ResourceData data)
        {
            if (!resources.ContainsKey(data.Type))
            {
                resources.Add(data.Type, data);
            }
        }

        public void SetCredits(float amount)
        {
            Credits = amount;
        }

        public void AddCredits(float amount)
        {
            Credits += amount;
            GameLogger.Log($"Credits added: {amount}. Total: {Credits}");
        }

        public bool SpendCredits(float amount)
        {
            if (Credits >= amount)
            {
                Credits -= amount;
                GameLogger.Log($"Credits spent: {amount}. Total: {Credits}");
                return true;
            }
            return false;
        }

        public void SetResource(ResourceType type, int quantity)
        {
             if (resources.ContainsKey(type))
            {
                resources[type].Quantity = quantity;
            }
        }

        public void AddResource(ResourceType type, int amount)
        {
            if (resources.ContainsKey(type))
            {
                resources[type].Quantity += amount;
                GameLogger.Log($"Added {amount} {type}. Total: {resources[type].Quantity}");
            }
        }

        public bool RemoveResource(ResourceType type, int amount)
        {
            if (resources.ContainsKey(type) && resources[type].Quantity >= amount)
            {
                resources[type].Quantity -= amount;
                return true;
            }
            return false;
        }

        public int GetResourceQuantity(ResourceType type)
        {
            return resources.ContainsKey(type) ? resources[type].Quantity : 0;
        }

        public ResourceData GetResourceData(ResourceType type)
        {
            return resources.ContainsKey(type) ? resources[type] : null;
        }

        public List<ResourceData> GetAllResources()
        {
            return new List<ResourceData>(resources.Values);
        }
    }
}
