using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SpaceRush.Systems;
using SpaceRush.Models;
using System.Text;

namespace SpaceRush.UI
{
    public class PlanetDashboardController : MonoBehaviour
    {
        [Header("Text Fields")]
        public TextMeshProUGUI planetNameText;
        public TextMeshProUGUI planetDetailsText; // Biome, Tech Required
        public TextMeshProUGUI resourceInfoText;  // Stockpile info

        [Header("Action Buttons")]
        public Button investigateButton;
        public Button mineButton;
        public Button upgradeMiningButton;
        public Button upgradeStationButton;
        public Button upgradeLogisticsButton;

        private Location _currentViewedLocation;

        private void Update()
        {
            if (LocationManager.Instance == null) return;

            // For now, Dashboard always shows the location the ship is AT.
            // Future feature: Click list to "View" without traveling.
            _currentViewedLocation = LocationManager.Instance.CurrentLocation;

            UpdateInfo();
            UpdateButtons();
        }

        private void UpdateInfo()
        {
            if (_currentViewedLocation == null)
            {
                planetNameText.text = "Deep Space";
                planetDetailsText.text = "No planet nearby.";
                resourceInfoText.text = "";
                return;
            }

            planetNameText.text = _currentViewedLocation.Name;

            // Details
            StringBuilder details = new StringBuilder();
            details.AppendLine($"Biome: {_currentViewedLocation.Biome}");
            details.AppendLine($"State: {_currentViewedLocation.State}");

            if (!string.IsNullOrEmpty(_currentViewedLocation.RequiredTechID))
            {
                details.AppendLine($"Tech Required: {_currentViewedLocation.RequiredTechID}");
            }
            planetDetailsText.text = details.ToString();

            // Resources / Infrastructure
            StringBuilder res = new StringBuilder();
            if (_currentViewedLocation.State == DiscoveryState.ReadyToMine)
            {
                res.AppendLine("Infrastructure:");
                res.AppendLine($"  Mining Lvl: {_currentViewedLocation.Infrastructure.MiningLevel}");
                res.AppendLine($"  Station Lvl: {_currentViewedLocation.Infrastructure.StationLevel}");
                res.AppendLine($"  Logistics Lvl: {_currentViewedLocation.Infrastructure.LogisticsLevel}");
                res.AppendLine("");
                res.AppendLine("Stockpile:");
                foreach(var kvp in _currentViewedLocation.Stockpile)
                {
                    res.AppendLine($"  {kvp.Key}: {kvp.Value}");
                }
            }
            else
            {
                res.AppendLine("No infrastructure established.");
            }
            resourceInfoText.text = res.ToString();
        }

        private void Start()
        {
            // Setup static listeners once
            if (investigateButton != null) investigateButton.onClick.AddListener(OnInvestigateClicked);
            if (mineButton != null) mineButton.onClick.AddListener(OnMineClicked);
            if (upgradeMiningButton != null) upgradeMiningButton.onClick.AddListener(() => OnUpgradeClicked("Mining"));
            if (upgradeStationButton != null) upgradeStationButton.onClick.AddListener(() => OnUpgradeClicked("Station"));
            if (upgradeLogisticsButton != null) upgradeLogisticsButton.onClick.AddListener(() => OnUpgradeClicked("Logistics"));
        }

        private void UpdateButtons()
        {
            if (_currentViewedLocation == null)
            {
                DisableAllButtons();
                return;
            }

            // Investigate Button
            bool canInvestigate = _currentViewedLocation.State == DiscoveryState.Discovered;
            if (investigateButton != null) investigateButton.gameObject.SetActive(canInvestigate);

            // Start Mining Button
            bool canMine = _currentViewedLocation.State == DiscoveryState.Investigated;
            if (mineButton != null) mineButton.gameObject.SetActive(canMine);

            // Upgrades
            bool hasInfra = _currentViewedLocation.State == DiscoveryState.ReadyToMine;
            if (upgradeMiningButton != null) upgradeMiningButton.gameObject.SetActive(hasInfra);
            if (upgradeStationButton != null) upgradeStationButton.gameObject.SetActive(hasInfra);
            if (upgradeLogisticsButton != null) upgradeLogisticsButton.gameObject.SetActive(hasInfra);
        }

        private void DisableAllButtons()
        {
            if (investigateButton != null) investigateButton.gameObject.SetActive(false);
            if (mineButton != null) mineButton.gameObject.SetActive(false);
            if (upgradeMiningButton != null) upgradeMiningButton.gameObject.SetActive(false);
            if (upgradeStationButton != null) upgradeStationButton.gameObject.SetActive(false);
            if (upgradeLogisticsButton != null) upgradeLogisticsButton.gameObject.SetActive(false);
        }

        // --- Event Handlers ---

        private void OnInvestigateClicked()
        {
            if (_currentViewedLocation != null)
                PlanetarySystem.Instance.InvestigatePlanet(_currentViewedLocation.ID);
        }

        private void OnMineClicked()
        {
            if (_currentViewedLocation != null)
                PlanetarySystem.Instance.StartMiningOperations(_currentViewedLocation.ID);
        }

        private void OnUpgradeClicked(string type)
        {
            if (_currentViewedLocation != null)
                PlanetarySystem.Instance.UpgradeInfrastructure(_currentViewedLocation.ID, type);
        }
    }
}
