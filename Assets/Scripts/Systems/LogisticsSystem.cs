using UnityEngine;
using SpaceRush.Core;
using SpaceRush.Models;
using System.Collections;
using System.Collections.Generic;

namespace SpaceRush.Systems
{
    public class LogisticsSystem : MonoBehaviour
    {
        public static LogisticsSystem Instance { get; private set; }

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
            StartCoroutine(AutomatedLogisticsLoop());
        }

        // Configuration for Cargo Preferences (Percentage 0.0 to 1.0)
        public Dictionary<ResourceType, float> CargoAllocations = new Dictionary<ResourceType, float>();

        public void ResetData()
        {
            CargoAllocations.Clear();
        }

        public void SetAllocation(ResourceType type, float percentage)
        {
            if (CargoAllocations.ContainsKey(type))
                CargoAllocations[type] = percentage;
            else
                CargoAllocations.Add(type, percentage);
        }

        // Called when the Main Ship visits a planet
        public void CollectLocalResources(LocationState loc)
        {
            if (loc == null || loc.Stockpile.Count == 0) return;

            int shipCapacity = FleetManager.Instance.CargoCapacity;

            // Calculate what to transfer based on capacity and allocations
            Dictionary<ResourceType, int> transferPlan = CalculateTransfer(loc.Stockpile, shipCapacity, CargoAllocations);

            // Execute Transfer
            foreach (var kvp in transferPlan)
            {
                ResourceType type = kvp.Key;
                int amount = kvp.Value;

                if (amount > 0)
                {
                    // Remove from Location
                    if (loc.Stockpile.ContainsKey(type))
                    {
                        loc.Stockpile[type] -= amount;
                    }

                    // Add to Global Inventory
                    ResourceManager.Instance.AddResource(type, amount);
                    GameLogger.Log($"Collected {amount} {type} from {loc.Definition.Name}");
                }
            }
        }

        /// <summary>
        /// Calculates the amount of each resource to transfer based on capacity and allocations.
        /// Public for Unit Testing.
        /// </summary>
        public Dictionary<ResourceType, int> CalculateTransfer(Dictionary<ResourceType, int> stockpile, int capacity, Dictionary<ResourceType, float> allocations)
        {
            Dictionary<ResourceType, int> toTransfer = new Dictionary<ResourceType, int>();

            // 1. Identify what is available to take
            List<ResourceType> availableTypes = new List<ResourceType>();
            foreach (var kvp in stockpile)
            {
                if (kvp.Value > 0) availableTypes.Add(kvp.Key);
            }

            if (availableTypes.Count == 0 || capacity <= 0) return toTransfer;

            // 2. Determine Strategy
            bool useAllocations = allocations != null && allocations.Count > 0;

            if (useAllocations)
            {
                // Strategy: Custom Allocations (Strict Quota)
                // "User would do percentage of cargo is what element"
                foreach (var kvp in allocations)
                {
                    ResourceType type = kvp.Key;
                    float pct = kvp.Value;

                    if (stockpile.ContainsKey(type))
                    {
                        int targetAmount = Mathf.FloorToInt(capacity * pct);
                        int available = stockpile[type];

                        // We take up to the quota, limited by availability
                        int amount = Mathf.Min(targetAmount, available);

                        if (amount > 0)
                        {
                            toTransfer[type] = amount;
                        }
                    }
                }
            }
            else
            {
                // Strategy: Smart Balance (Fair Share + Fill Remaining)

                // Clone stockpile to track remaining available during calculation
                Dictionary<ResourceType, int> tempStock = new Dictionary<ResourceType, int>(stockpile);
                int remainingCapacity = capacity;

                // Pass 1: Fair Share
                int initialCount = availableTypes.Count;
                int share = remainingCapacity / initialCount;

                if (share > 0)
                {
                    foreach(var type in availableTypes)
                    {
                        int available = tempStock[type];
                        int take = Mathf.Min(share, available);

                        if (take > 0)
                        {
                            toTransfer[type] = take;
                            remainingCapacity -= take;
                            tempStock[type] -= take;
                        }
                    }
                }

                // Pass 2: Fill Remaining (Greedy Loop)
                while (remainingCapacity > 0)
                {
                    // Find who still has stock
                    List<ResourceType> remainingTypes = new List<ResourceType>();
                    foreach(var type in availableTypes)
                    {
                        if (tempStock[type] > 0) remainingTypes.Add(type);
                    }

                    if (remainingTypes.Count == 0) break;

                    int subShare = Mathf.Max(1, remainingCapacity / remainingTypes.Count);

                    int takenThisRound = 0;
                    foreach(var type in remainingTypes)
                    {
                        if (remainingCapacity == 0) break;

                        int available = tempStock[type];
                        int take = Mathf.Min(subShare, available);
                        take = Mathf.Min(take, remainingCapacity);

                        if (take > 0)
                        {
                            if (toTransfer.ContainsKey(type)) toTransfer[type] += take;
                            else toTransfer[type] = take;

                            remainingCapacity -= take;
                            tempStock[type] -= take;
                            takenThisRound += take;
                        }
                    }

                    if (takenThisRound == 0) break;
                }
            }

            return toTransfer;
        }

        private IEnumerator AutomatedLogisticsLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(5.0f);

                // Check for Automated Logistics Tech
                if (ResearchManager.Instance.IsTechUnlocked("AUTO_LOGISTICS"))
                {
                    if (LocationManager.Instance != null && LocationManager.Instance.Locations != null)
                    {
                        foreach (var loc in LocationManager.Instance.Locations)
                        {
                            if (loc.State == DiscoveryState.ReadyToMine && loc.Infrastructure.LogisticsLevel >= 1)
                            {
                                int transferRate = loc.Infrastructure.LogisticsLevel * 5;

                                List<ResourceType> keys = new List<ResourceType>(loc.Stockpile.Keys);
                                foreach (var type in keys)
                                {
                                    int available = loc.Stockpile[type];
                                    int toTransfer = Mathf.Min(available, transferRate);

                                    if (toTransfer > 0)
                                    {
                                        loc.Stockpile[type] -= toTransfer;
                                        ResourceManager.Instance.AddResource(type, toTransfer);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
