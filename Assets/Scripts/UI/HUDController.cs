using UnityEngine;
using TMPro;
using SpaceRush.Core;
using SpaceRush.Systems;

namespace SpaceRush.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI creditsText;
        public TextMeshProUGUI shipStatusText;
        public TextMeshProUGUI shipHealthText;

        private void Update()
        {
            if (GameManager.Instance == null) return;

            UpdateCredits();
            UpdateShipStatus();
        }

        private void UpdateCredits()
        {
            if (creditsText != null && ResourceManager.Instance != null)
            {
                creditsText.text = $"Credits: {ResourceManager.Instance.Credits:F0}";
            }
        }

        private void UpdateShipStatus()
        {
            if (FleetManager.Instance == null) return;

            if (shipHealthText != null)
            {
                float health = FleetManager.Instance.RepairStatus;
                shipHealthText.text = $"Ship Integrity: {health:F1}%";
            }

            if (shipStatusText != null)
            {
                // Simple status logic based on location
                // If moving, we might need a flag in FleetManager or LocationManager (e.g. IsTraveling)
                // For now, check if LocationManager has a current location

                // Note: LocationManager.TryTravel sets CurrentLocation immediately in the current logic,
                // but real travel might take time. For now, we display the current location name.
                string locName = LocationManager.Instance.CurrentLocation != null ?
                    LocationManager.Instance.CurrentLocation.Name : "Deep Space";

                shipStatusText.text = $"Location: {locName}";
            }
        }
    }
}
