using UnityEngine;

namespace SpaceRush.Systems
{
    public class FleetManager : MonoBehaviour
    {
        public static FleetManager Instance { get; private set; }

        public int ShipLevel { get; private set; } = 1;
        public float MiningSpeed { get; private set; } = 1.0f;
        public int CargoCapacity { get; private set; } = 10;

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
            ShipLevel++;
            MiningSpeed *= 1.2f;
            CargoCapacity += 10;
            Debug.Log($"Ship Upgraded to Level {ShipLevel}. Mining Speed: {MiningSpeed}, Capacity: {CargoCapacity}");
        }
    }
}
