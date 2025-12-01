using UnityEngine;
using SpaceRush.Core;
using SpaceRush.Models;
using System.Collections;

namespace SpaceRush.Systems
{
    public class TradingSystem : MonoBehaviour
    {
        public static TradingSystem Instance { get; private set; }

        private float marketUpdateInterval = 30f; // Update market prices every 30 seconds
        private float autoSellInterval = 5f;

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
            StartCoroutine(UpdateMarketPrices());
            StartCoroutine(AutoSellLoop());
        }

        private IEnumerator UpdateMarketPrices()
        {
            while (true)
            {
                foreach (var resource in ResourceManager.Instance.GetAllResources())
                {
                    // Fluctuate price by +/- 10%
                    float fluctuation = Random.Range(0.9f, 1.1f);
                    resource.CurrentMarketValue = resource.BaseValue * fluctuation;
                    Debug.Log($"Market Update: {resource.Name} is now worth {resource.CurrentMarketValue:F2}");
                }
                yield return new WaitForSeconds(marketUpdateInterval);
            }
        }

        private IEnumerator AutoSellLoop()
        {
            while (true)
            {
                SellAllResources();
                yield return new WaitForSeconds(autoSellInterval);
            }
        }

        public void SellAllResources()
        {
            float totalEarnings = 0f;
            foreach (var resource in ResourceManager.Instance.GetAllResources())
            {
                if (resource.Quantity > 0)
                {
                    float value = resource.Quantity * resource.CurrentMarketValue;
                    totalEarnings += value;
                    ResourceManager.Instance.RemoveResource(resource.Type, resource.Quantity);
                }
            }

            if (totalEarnings > 0)
            {
                ResourceManager.Instance.AddCredits(totalEarnings);
                Debug.Log($"Sold all resources for {totalEarnings:F2} credits.");
            }
        }
    }
}
