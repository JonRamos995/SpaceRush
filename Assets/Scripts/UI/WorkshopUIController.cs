using UnityEngine;
using System.Collections.Generic;
using SpaceRush.Systems;

namespace SpaceRush.UI
{
    public class WorkshopUIController : MonoBehaviour
    {
        [Header("Configuration")]
        public GameObject WorkshopWindow;
        public GameObject SlotPrefab;
        public Transform SlotContainer;

        private List<WorkshopSlotUI> slotUIs = new List<WorkshopSlotUI>();

        private void Start()
        {
            if (WorkshopWindow != null) WorkshopWindow.SetActive(false);
        }

        private void Update()
        {
            if (WorkshopWindow != null && WorkshopWindow.activeSelf)
            {
                RefreshSlots();
            }
        }

        public void ToggleWindow()
        {
            if (WorkshopWindow != null)
            {
                bool isActive = !WorkshopWindow.activeSelf;
                WorkshopWindow.SetActive(isActive);
                if (isActive)
                {
                    RebuildSlotList();
                }
            }
        }

        private void RebuildSlotList()
        {
            // Clear existing UIs from list
            slotUIs.Clear();

            // Destroy all children in container
            if (SlotContainer != null)
            {
                foreach (Transform child in SlotContainer)
                {
                    Destroy(child.gameObject);
                }
            }

            // Create new
            if (WorkshopManager.Instance != null && SlotPrefab != null && SlotContainer != null)
            {
                for (int i = 0; i < WorkshopManager.Instance.Slots.Count; i++)
                {
                    GameObject obj = Instantiate(SlotPrefab, SlotContainer);
                    WorkshopSlotUI ui = obj.GetComponent<WorkshopSlotUI>();
                    if (ui != null)
                    {
                        ui.Setup(i, this);
                        slotUIs.Add(ui);
                    }
                }
            }
        }

        private void RefreshSlots()
        {
            // Check for count mismatch (e.g. upgraded ship while window open)
            if (WorkshopManager.Instance != null && WorkshopManager.Instance.Slots.Count != slotUIs.Count)
            {
                RebuildSlotList();
            }

            foreach (var ui in slotUIs)
            {
                ui.UpdateUI();
            }
        }
    }
}
