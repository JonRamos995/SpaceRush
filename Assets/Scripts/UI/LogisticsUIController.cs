using UnityEngine;
using System.Collections.Generic;
using SpaceRush.Models;
using SpaceRush.Systems;
using SpaceRush.Core;

namespace SpaceRush.UI
{
    public class LogisticsUIController : MonoBehaviour
    {
        [Header("Configuration")]
        public GameObject LogisticsItemPrefab;
        public Transform ContentContainer;
        public GameObject PanelRoot; // To show/hide

        private List<LogisticsItemUI> items = new List<LogisticsItemUI>();

        public void Show()
        {
            if (PanelRoot != null) PanelRoot.SetActive(true);
            RefreshList();
        }

        public void Hide()
        {
            if (PanelRoot != null) PanelRoot.SetActive(false);
        }

        private void Start()
        {
            // Start hidden
            Hide();
        }

        private void RefreshList()
        {
            // Clear existing
            foreach (Transform child in ContentContainer)
            {
                Destroy(child.gameObject);
            }
            items.Clear();

            if (LogisticsSystem.Instance == null || ResourceManager.Instance == null) return;

            var allocations = LogisticsSystem.Instance.CargoAllocations;

            foreach (var res in ResourceManager.Instance.GetAllResources())
            {
                // Determine current allocation
                float currentPct = 0f;
                if (allocations.ContainsKey(res.Type))
                {
                    currentPct = allocations[res.Type];
                }

                if (LogisticsItemPrefab != null)
                {
                    GameObject obj = Instantiate(LogisticsItemPrefab, ContentContainer);
                    LogisticsItemUI ui = obj.GetComponent<LogisticsItemUI>();
                    if (ui != null)
                    {
                        ui.Setup(res.Type, currentPct, this);
                        items.Add(ui);
                    }
                }
            }
        }

        public void OnAllocationChanged(ResourceType type, float pct)
        {
            if (LogisticsSystem.Instance != null)
            {
                LogisticsSystem.Instance.SetAllocation(type, pct);
            }
        }
    }
}
