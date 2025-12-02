using UnityEngine;
using SpaceRush.Core;
using SpaceRush.Models;
using System.Collections;
using System.Collections.Generic;

namespace SpaceRush.Systems
{
    public class TradingSystem : MonoBehaviour
    {
        public static TradingSystem Instance { get; private set; }

        private float marketUpdateInterval = 30f; // Update market prices every 30 seconds

        // Market Trends
        private Dictionary<ResourceType, float> priceMultipliers = new Dictionary<ResourceType, float>();

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
            StartCoroutine(UpdateMarketPrices());
        }

        [System.Serializable]
        public struct MarketEvent
        {
            public string Name;
            public float Duration;
            public ResourceType TargetResource;
            public float PriceMultiplier;
        }

        public MarketEvent? CurrentEvent { get; private set; }
        private float eventTimer = 0f;

        private void Update()
        {
            if (CurrentEvent.HasValue)
            {
                eventTimer -= Time.deltaTime;
                if (eventTimer <= 0)
                {
                    GameLogger.Log($"Market Event Ended: {CurrentEvent.Value.Name}");
                    CurrentEvent = null;
                }
            }
        }

        private IEnumerator UpdateMarketPrices()
        {
            while (true)
            {
                // Event Trigger Chance
                if (!CurrentEvent.HasValue && UnityEngine.Random.value < 0.2f)
                {
                    TriggerRandomEvent();
                }

                foreach (var resource in ResourceManager.Instance.GetAllResources())
                {
                    // 1. Random fluctuation +/- 5%
                    float fluctuation = UnityEngine.Random.Range(0.95f, 1.05f);

                    // 2. Trend Logic (Simple random walk for now)
                    if (!priceMultipliers.ContainsKey(resource.Type)) priceMultipliers[resource.Type] = 1.0f;

                    // Slightly drift the trend
                    priceMultipliers[resource.Type] *= UnityEngine.Random.Range(0.98f, 1.02f);
                    priceMultipliers[resource.Type] = Mathf.Clamp(priceMultipliers[resource.Type], 0.5f, 2.0f); // Cap between 50% and 200%

                    float eventMult = 1.0f;
                    if (CurrentEvent.HasValue && CurrentEvent.Value.TargetResource == resource.Type)
                    {
                        eventMult = CurrentEvent.Value.PriceMultiplier;
                    }

                    resource.CurrentMarketValue = resource.BaseValue * fluctuation * priceMultipliers[resource.Type] * eventMult;

                    // Ensure it doesn't go negative or too low
                    if (resource.CurrentMarketValue < 1) resource.CurrentMarketValue = 1;

                    // Debug.Log($"Market Update: {resource.Name} is now worth {resource.CurrentMarketValue:F2} (Trend: {priceMultipliers[resource.Type]:F2})");
                }
                yield return new WaitForSeconds(marketUpdateInterval);
            }
        }

        private void TriggerRandomEvent()
        {
            var resources = ResourceManager.Instance.GetAllResources();
            if (resources.Count == 0) return;

            var target = resources[UnityEngine.Random.Range(0, resources.Count)];
            bool isBoom = UnityEngine.Random.value > 0.5f;
            float mult = isBoom ? UnityEngine.Random.Range(1.5f, 3.0f) : UnityEngine.Random.Range(0.3f, 0.7f);
            string name = isBoom ? $"{target.Name} High Demand" : $"{target.Name} Market Surplus";

            CurrentEvent = new MarketEvent
            {
                Name = name,
                Duration = 60f,
                TargetResource = target.Type,
                PriceMultiplier = mult
            };
            eventTimer = 60f;
            GameLogger.Log($"MARKET ALERT: {name}! Prices impacted for 60s.");
        }

        public float GetSellPrice(ResourceType type)
        {
            var res = ResourceManager.Instance.GetResourceData(type);
            if (res == null) return 0;
            return res.CurrentMarketValue;
        }

        public float GetBuyPrice(ResourceType type)
        {
            // Buy price is slightly higher than sell price (Spread)
            return GetSellPrice(type) * 1.1f;
        }

        public void SellResource(ResourceType type, int amount)
        {
            var res = ResourceManager.Instance.GetResourceData(type);
            if (res == null || amount <= 0) return;

            if (res.Quantity >= amount)
            {
                float unitPrice = GetSellPrice(type);
                float totalValue = amount * unitPrice;

                ResourceManager.Instance.RemoveResource(type, amount);
                ResourceManager.Instance.AddCredits(totalValue);

                GameLogger.Log($"Sold {amount} {res.Name} for {totalValue:F1} CR.");
            }
            else
            {
                GameLogger.Log("Not enough resources to sell.");
            }
        }

        public void BuyResource(ResourceType type, int amount)
        {
            var res = ResourceManager.Instance.GetResourceData(type);
            if (res == null || amount <= 0) return;

            float unitPrice = GetBuyPrice(type);
            float totalCost = amount * unitPrice;

            if (ResourceManager.Instance.SpendCredits(totalCost))
            {
                ResourceManager.Instance.AddResource(type, amount);
                GameLogger.Log($"Bought {amount} {res.Name} for {totalCost:F1} CR.");
            }
            else
            {
                GameLogger.Log("Not enough credits.");
            }
        }
    }
}
