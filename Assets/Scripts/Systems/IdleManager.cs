using UnityEngine;
using System;
using SpaceRush.Core;
using SpaceRush.Models;

namespace SpaceRush.Systems
{
    public class IdleManager : MonoBehaviour
    {
        public static IdleManager Instance { get; private set; }

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

        // Removed PlayerPrefs logic in favor of centralized SaveData

        public void CalculateOfflineProgressFromTimestamp(long timestamp)
        {
            if (timestamp == 0) return; // New game

            DateTime lastLogin = DateTime.FromBinary(timestamp);
            TimeSpan timeAway = DateTime.UtcNow - lastLogin;
            double secondsAway = timeAway.TotalSeconds;

            GameLogger.Log($"Player was away for {secondsAway} seconds.");

            if (secondsAway > 10)
            {
                // Offline Logic:
                // 1. Calculate potential yield based on Fleet Mining Speed
                float fleetMiningPower = FleetManager.Instance.MiningSpeed; // Per second (approx)
                float totalMiningPower = fleetMiningPower * (float)secondsAway * 0.5f; // 50% efficiency for offline

                // 2. Determine where to mine
                // Since we don't strictly save "Ship Location" in a dedicated field yet (it's part of LocationManager state but not explicitly "Ship is Here"),
                // we will use the CurrentLocation from LocationManager, which was just loaded.

                LocationState currentLoc = LocationManager.Instance.CurrentLocation;

                if (currentLoc != null && currentLoc.State >= DiscoveryState.ReadyToMine && currentLoc.Definition.AvailableResources.Count > 0)
                {
                    // Distribute mining power among available resources
                    int resourceCount = currentLoc.Definition.AvailableResources.Count;
                    int amountPerResource = Mathf.FloorToInt(totalMiningPower / resourceCount);

                    if (amountPerResource > 0)
                    {
                        foreach (var resType in currentLoc.Definition.AvailableResources)
                        {
                            ResourceManager.Instance.AddResource(resType, amountPerResource);
                        }
                        GameLogger.Log($"Offline Progress: Mined {amountPerResource} of each local resource while away ({secondsAway:F0}s).");
                    }
                }
                else
                {
                     // Fallback: Give some credits representing "Odd jobs" or trading while offline
                     float creditGain = (float)secondsAway * 1.0f; // 1 credit per second
                     ResourceManager.Instance.AddCredits(creditGain);
                     GameLogger.Log($"Offline Progress: Earned {creditGain:F0} credits from automated trading while away.");
                }
            }
        }
    }
}
