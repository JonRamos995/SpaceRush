using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SpaceRush.Systems;
using System.Collections.Generic;

namespace SpaceRush.UI
{
    public class LocationListController : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject locationButtonPrefab;
        public Transform listContainer;

        private List<GameObject> _spawnedButtons = new List<GameObject>();

        private void Start()
        {
            // Initial population
            RefreshList();
        }

        private void Update()
        {
            // Ideally we use events when discovery happens, but polling is safer for this prototype phase
            // to ensure UI stays in sync if we missed an event hook.
            // Optimization: Only refresh if count changes or discovery state changes.
        }

        public void RefreshList()
        {
            if (LocationManager.Instance == null) return;

            // Clear old buttons
            foreach (var btn in _spawnedButtons)
            {
                Destroy(btn);
            }
            _spawnedButtons.Clear();

            // Spawn new ones
            foreach (var loc in LocationManager.Instance.Locations)
            {
                if (loc.State == DiscoveryState.Hidden) continue; // Don't show hidden planets

                GameObject newBtn = Instantiate(locationButtonPrefab, listContainer);
                _spawnedButtons.Add(newBtn);

                // Setup Button Text
                TextMeshProUGUI btnText = newBtn.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null)
                {
                    btnText.text = loc.Name;
                    if (loc == LocationManager.Instance.CurrentLocation)
                    {
                        btnText.text += " [HERE]";
                    }
                }

                // Setup Click Listener
                Button btnComp = newBtn.GetComponent<Button>();
                if (btnComp != null)
                {
                    string locId = loc.ID;
                    btnComp.onClick.AddListener(() => OnLocationClicked(locId));
                }
            }
        }

        private void OnLocationClicked(string locId)
        {
            // Request Travel
            LocationManager.Instance.TryTravel(locId);

            // Refresh to update "[HERE]" marker
            RefreshList();
        }
    }
}
