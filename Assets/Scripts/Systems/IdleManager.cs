using UnityEngine;
using System;
using System.Collections.Generic;
using SpaceRush.Core;
using SpaceRush.Models;

namespace SpaceRush.Systems
{
    public class IdleManager : MonoBehaviour
    {
        public static IdleManager Instance { get; private set; }

        public class OfflineGainData
        {
            public float Credits;
            public Dictionary<ResourceType, int> Resources = new Dictionary<ResourceType, int>();
        }

        public OfflineGainData LastGains { get; private set; }

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

        public void CalculateOfflineProgressFromTimestamp(long timestamp)
        {
            if (timestamp == 0) return; // New game

            DateTime lastLogin = DateTime.FromBinary(timestamp);
            TimeSpan timeAway = DateTime.UtcNow - lastLogin;
            double secondsAway = timeAway.TotalSeconds;

            GameLogger.Log($"Player was away for {secondsAway} seconds.");

            if (secondsAway > 10)
            {
                LastGains = new OfflineGainData();

                // Offline Logic:
                // 1. Calculate potential yield based on Fleet Mining Speed
                float fleetMiningPower = FleetManager.Instance.MiningSpeed; // Per second (approx)
                float totalMiningPower = fleetMiningPower * (float)secondsAway * 0.5f; // 50% efficiency for offline

                LocationState currentLoc = LocationManager.Instance.CurrentLocation;

                if (currentLoc != null && currentLoc.State >= DiscoveryState.ReadyToMine && currentLoc.Definition.AvailableResources.Count > 0)
                {
                    int resourceCount = currentLoc.Definition.AvailableResources.Count;
                    int amountPerResource = Mathf.FloorToInt(totalMiningPower / resourceCount);

                    if (amountPerResource > 0)
                    {
                        foreach (var resType in currentLoc.Definition.AvailableResources)
                        {
                            ResourceManager.Instance.AddResource(resType, amountPerResource);
                            LastGains.Resources[resType] = amountPerResource;
                        }
                        GameLogger.Log($"Offline Progress: Mined {amountPerResource} of each local resource while away ({secondsAway:F0}s).");
                    }
                }
                else
                {
                     float creditGain = (float)secondsAway * 1.0f; // 1 credit per second
                     ResourceManager.Instance.AddCredits(creditGain);
                     LastGains.Credits = creditGain;
                     GameLogger.Log($"Offline Progress: Earned {creditGain:F0} credits from automated trading while away.");
                }

                GameLogger.Log("[AdMob] Opportunity: Call DoubleOfflineGains() to double these rewards!");
            }
        }

        public void DoubleOfflineGains()
        {
            if (LastGains == null) return;

            AdManager.Instance.WatchAd("DoubleOffline", () => {
                if (LastGains.Credits > 0)
                {
                    ResourceManager.Instance.AddCredits(LastGains.Credits);
                    GameLogger.Log($"[AdMob] Doubled Credits! +{LastGains.Credits}");
                }
                foreach(var kvp in LastGains.Resources)
                {
                    ResourceManager.Instance.AddResource(kvp.Key, kvp.Value);
                    GameLogger.Log($"[AdMob] Doubled {kvp.Key}! +{kvp.Value}");
                }
                // Clear to prevent infinite doubling
                LastGains = null;
            });
        }
    }
}
