using UnityEngine;
using System.Collections.Generic;
using SpaceRush.Models;
using SpaceRush.Systems;
using SpaceRush.Core;

namespace SpaceRush.UI
{
    public class MarketUIController : MonoBehaviour
    {
        [Header("Configuration")]
        public GameObject MarketItemPrefab;
        public Transform ContentContainer;

        private List<MarketItemUI> items = new List<MarketItemUI>();
        private float updateTimer = 0f;

        private void Start()
        {
            InitializeMarket();
        }

        private void InitializeMarket()
        {
            // Clear existing
            foreach (Transform child in ContentContainer)
            {
                Destroy(child.gameObject);
            }
            items.Clear();

            // Create an item for each resource
            foreach (var res in ResourceManager.Instance.GetAllResources())
            {
                GameObject obj = Instantiate(MarketItemPrefab, ContentContainer);
                MarketItemUI ui = obj.GetComponent<MarketItemUI>();
                if (ui != null)
                {
                    ui.Setup(res.Type);
                    items.Add(ui);
                }
            }
        }

        private void Update()
        {
            // Update UI every 0.5s to save performance, or every frame if needed.
            updateTimer += Time.deltaTime;
            if (updateTimer > 0.5f)
            {
                updateTimer = 0f;
                RefreshMarket();
            }
        }

        private void RefreshMarket()
        {
            foreach (var item in items)
            {
                item.UpdateUI();
            }
        }
    }
}
