using UnityEngine;

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

        public void UpgradeShip()
        {
            if (!IsOperational)
            {
                Debug.Log("Cannot upgrade. Ship is damaged.");
                return;
            }

            ShipLevel++;
            MiningSpeed *= 1.2f;
            CargoCapacity += 10;
            Debug.Log($"Ship Upgraded to Level {ShipLevel}. Mining Speed: {MiningSpeed}, Capacity: {CargoCapacity}");
        }

        public void RepairShip(float progressAmount)
        {
            if (IsOperational) return;

            RepairStatus += progressAmount;
            if (RepairStatus >= 100f)
            {
                RepairStatus = 100f;
                Debug.Log("Ship Repairs Complete! Systems Online. Ready for launch.");
            }
            else
            {
                Debug.Log($"Repair Progress: {RepairStatus:F1}%");
            }
        }
    }
}
