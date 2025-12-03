using UnityEngine;
using System;
using SpaceRush.Core;

namespace SpaceRush.Systems
{
    public class AdManager : MonoBehaviour
    {
        public static AdManager Instance { get; private set; }

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

        public void WatchAd(string placementId, Action onReward)
        {
            GameLogger.Log($"[AdMob] Requesting Ad for {placementId}...");

            // Simulation: In a real build, this shows an ad.
            // Here we just simulate success.
            // For UI, we might pop a dialog.

            // Simulating successful watch:
            GameLogger.Log("[AdMob] Ad Watched Successfully!");
            onReward?.Invoke();
        }
    }
}
