using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SpaceRush.Systems;
using SpaceRush.Core;

namespace SpaceRush.UI
{
    public class ShipUIController : MonoBehaviour
    {
        [Header("References")]
        public GameObject shipWindowPanel;
        public TMP_Text shipStatsText; // Level, Speed, Capacity
        public TMP_Text repairStatusText;

        [Header("Actions")]
        public Button repairButton;
        public Button upgradeButton;
        public TMP_Text upgradeCostText;
        public Button closeButton;

        private void Start()
        {
            if (repairButton != null)
                repairButton.onClick.AddListener(OnRepairClicked);

            if (upgradeButton != null)
                upgradeButton.onClick.AddListener(OnUpgradeClicked);

            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            Hide();
        }

        private void Update()
        {
            if (shipWindowPanel != null && shipWindowPanel.activeSelf)
            {
                UpdateUI();
            }
        }

        public void Show()
        {
            if (shipWindowPanel != null)
                shipWindowPanel.SetActive(true);
        }

        public void Hide()
        {
            if (shipWindowPanel != null)
                shipWindowPanel.SetActive(false);
        }

        private void UpdateUI()
        {
            if (FleetManager.Instance == null) return;

            // Stats
            if (shipStatsText != null)
            {
                shipStatsText.text = $"Level: {FleetManager.Instance.ShipLevel}\n" +
                                     $"Mining Speed: {FleetManager.Instance.MiningSpeed:F1}x\n" +
                                     $"Cargo Capacity: {FleetManager.Instance.CargoCapacity} tons";
            }

            // Repair
            float repairStatus = FleetManager.Instance.RepairStatus;
            if (repairStatusText != null)
            {
                repairStatusText.text = FleetManager.Instance.IsOperational
                    ? "SYSTEMS ONLINE"
                    : $"DAMAGED ({repairStatus:F1}%)";

                repairStatusText.color = FleetManager.Instance.IsOperational ? Color.green : Color.red;
            }

            if (repairButton != null)
            {
                repairButton.interactable = !FleetManager.Instance.IsOperational;
                // Maybe show cost?
                // FleetManager.Instance.RepairCostPerTick is for auto-repair.
                // We'll implement an "Instant Repair" for a lump sum here or just toggle auto-repair?
                // The current logic in GameManager uses RepairCostPerTick.
                // Let's make this button do a "Emergency Repair" chunk.
            }

            // Upgrade
            float upgradeCost = FleetManager.Instance.GetUpgradeCost();
            if (upgradeCostText != null)
                upgradeCostText.text = $"Upgrade ({upgradeCost:F0} CR)";

            if (upgradeButton != null)
            {
                upgradeButton.interactable = FleetManager.Instance.IsOperational &&
                                             ResourceManager.Instance.Credits >= upgradeCost;
            }
        }

        private void OnRepairClicked()
        {
            // Manual repair action
            // Repairs 10% for cost
            float repairAmount = 10f;
            float cost = 100f; // Flat fee for manual intervention?

            if (ResourceManager.Instance.SpendCredits(cost))
            {
                FleetManager.Instance.RepairShip(repairAmount);
                GameLogger.Log("Manual repairs initiated.");
            }
            else
            {
                GameLogger.Log("Not enough credits for manual repair.");
            }
        }

        private void OnUpgradeClicked()
        {
            float upgradeCost = FleetManager.Instance.GetUpgradeCost();
            if (ResourceManager.Instance.SpendCredits(upgradeCost))
            {
                FleetManager.Instance.UpgradeShip();
                GameLogger.Log($"Ship upgraded for {upgradeCost} credits!");
            }
        }
    }
}
