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
        public void CollectLocalResources(LocationState loc)
        {
            if (loc == null || loc.Stockpile.Count == 0) return;

            int shipCapacity = FleetManager.Instance.CargoCapacity;

            // TODO: Implement capacity check

            List<ResourceType> keys = new List<ResourceType>(loc.Stockpile.Keys);
            foreach (var type in keys)
            {
                int amount = loc.Stockpile[type];
                if (amount > 0)
                {
                    ResourceManager.Instance.AddResource(type, amount);
                    loc.Stockpile[type] = 0;
                    Debug.Log($"Collected {amount} {type} from {loc.Definition.Name}");
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
