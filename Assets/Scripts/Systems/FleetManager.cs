using UnityEngine;
using SpaceRush.Models;
using SpaceRush.Core;

namespace SpaceRush.Systems
{
    public class FleetManager : MonoBehaviour
    {
        public static FleetManager Instance { get; private set; }

        public int ShipLevel { get; private set; } = 1;
        public float MiningSpeed { get; private set; } = 1.0f;
        public int CargoCapacity { get; private set; } = 10;

        // Repair Mechanics
        public float RepairStatus { get; private set; } = 0.0f; // 0% to 100%
        public bool IsOperational => RepairStatus >= 100f;
        public float RepairCostPerTick { get; private set; } = 10f; // Credits needed per tick to repair

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

        public float GetUpgradeCost()
        {
            return ShipLevel * 1000f;
        }

        public void UpgradeShip()
        {
            if (!IsOperational)
            {
                GameLogger.Log("Cannot upgrade. Ship is damaged.");
                return;
            }

            float cost = GetUpgradeCost();
            if (ResourceManager.Instance.SpendCredits(cost))
            {
                ShipLevel++;
                RecalculateStats();
                GameLogger.Log($"Ship Upgraded to Level {ShipLevel}. Mining Speed: {MiningSpeed}, Capacity: {CargoCapacity}");
            }
            else
            {
                GameLogger.Log($"Not enough credits to upgrade ship. Cost: {cost}, Current: {ResourceManager.Instance.Credits}");
            }
        }

        public void RepairShip(float progressAmount)
        {
            if (IsOperational) return;

            RepairStatus += progressAmount;
            if (RepairStatus >= 100f)
            {
                RepairStatus = 100f;
                GameLogger.Log("Ship Repairs Complete! Systems Online. Ready for launch.");
            }
            else
            {
                GameLogger.Log($"Repair Progress: {RepairStatus:F1}%");
            }
        }

        public void ResetData()
        {
            ShipLevel = 1;
            RepairStatus = 0.0f;
            RecalculateStats();
        }

        public void LoadData(FleetSaveData data)
        {
            if (data == null) return;
            ShipLevel = data.ShipLevel;
            RepairStatus = data.RepairStatus;

            RecalculateStats();
        }

        public void RecalculateStats()
        {
            // Base Stats
            float baseSpeed = 1.0f * Mathf.Pow(1.2f, ShipLevel - 1);
            int baseCapacity = 10 * ShipLevel;

            // Apply Tech Bonuses
            if (ResearchManager.Instance.IsTechUnlocked("EFFICIENCY_1"))
            {
                baseSpeed *= 1.1f; // +10%
            }

            // Apply Civilization Bonus
            if (CivilizationManager.Instance != null)
            {
                baseSpeed *= CivilizationManager.Instance.Multiplier;
            }

            MiningSpeed = baseSpeed;
            CargoCapacity = baseCapacity;
        }
    }
}
