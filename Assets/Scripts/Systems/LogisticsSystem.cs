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

        private void Start()
        {
            StartCoroutine(AutomatedLogisticsLoop());
        }

        // Called when the Main Ship visits a planet
        public void CollectLocalResources(Location loc)
        {
            if (loc == null || loc.Stockpile.Count == 0) return;

            // Move everything from Local Stockpile to Global ResourceManager
            // In a more complex version, we would check CargoCapacity of the fleet
            // For now, let's assume the ship fills up to capacity and leaves the rest

            int shipCapacity = FleetManager.Instance.CargoCapacity;
            int currentShipLoad = 0; // We assume ship empties on "Sell" or "Return to Earth".
                                     // But ResourceManager is global credits.
                                     // For simplicity in this blueprint:
                                     // "Collecting" moves resources to a virtual "Ship Hold" or directly to Global Inventory?
                                     // The prompt says "sell them for a high price... automatically and idly".
                                     // Let's assume collecting adds to Global Inventory (which TradingSystem then sells).

            // To simulate capacity, we might limit how much we can take.
            // But let's keep it fluid: Take all allowed by LogisticsLevel/FleetCapacity

            List<ResourceType> keys = new List<ResourceType>(loc.Stockpile.Keys);
            foreach (var type in keys)
            {
                int amount = loc.Stockpile[type];
                if (amount > 0)
                {
                    ResourceManager.Instance.AddResource(type, amount);
                    loc.Stockpile[type] = 0;
                    Debug.Log($"Collected {amount} {type} from {loc.Name}");
                }
            }
        }

        private IEnumerator AutomatedLogisticsLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(5.0f);

                // Check for Automated Logistics Tech
                if (ResearchManager.Instance.IsTechUnlocked("AUTO_LOGISTICS"))
                {
                    foreach (var loc in LocationManager.Instance.Locations)
                    {
                        if (loc.State == DiscoveryState.ReadyToMine && loc.Infrastructure.LogisticsLevel > 1)
                        {
                            // If Logistics Level is high enough, automatic shuttles bring resources back
                            // Amount depends on LogisticsLevel
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
                                    // Debug.Log($"Auto-Logistics: Transferred {toTransfer} {type} from {loc.Name}");
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
