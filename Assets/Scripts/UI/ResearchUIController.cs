using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SpaceRush.Systems;
using System.Collections.Generic;

namespace SpaceRush.UI
{
    public class ResearchUIController : MonoBehaviour
    {
        [Header("References")]
        public GameObject researchWindowPanel;
        public TMP_Text researchersText;
        public TMP_Text researchPointsText;
        public Button hireResearcherButton;
        public Button closeButton;

        [Header("Tech List")]
        public Transform techListContent; // Scroll View Content
        public GameObject techItemPrefab; // Prefab to instantiate

        private void Start()
        {
            if (hireResearcherButton != null)
                hireResearcherButton.onClick.AddListener(HireResearcher);

            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            // Hide by default
            if (researchWindowPanel != null)
                researchWindowPanel.SetActive(false);
        }

        private void Update()
        {
            if (researchWindowPanel != null && researchWindowPanel.activeSelf)
            {
                UpdateUI();
            }
        }

        public void Show()
        {
            if (researchWindowPanel != null)
            {
                researchWindowPanel.SetActive(true);
                RefreshTechList();
            }
        }

        public void Hide()
        {
            if (researchWindowPanel != null)
                researchWindowPanel.SetActive(false);
        }

        private void UpdateUI()
        {
            if (ResearchManager.Instance == null) return;

            if (researchersText != null)
                researchersText.text = $"Researchers: {ResearchManager.Instance.Researchers}";

            if (researchPointsText != null)
                researchPointsText.text = $"RP: {ResearchManager.Instance.ResearchPoints:F0}";

            // Note: We might want to update buttons interactability based on credits/RP
        }

        private void HireResearcher()
        {
            if (ResearchManager.Instance != null)
                ResearchManager.Instance.HireResearcher();
        }

        private void RefreshTechList()
        {
            // Clear existing
            foreach (Transform child in techListContent)
            {
                Destroy(child.gameObject);
            }

            // Get all techs (we need to expose them in ResearchManager, currently private list)
            // I will update ResearchManager to expose a ReadOnly list of techs
            List<Technology> techs = ResearchManager.Instance.GetAllTechnologies();

            foreach (var tech in techs)
            {
                if (techItemPrefab != null)
                {
                    GameObject itemObj = Instantiate(techItemPrefab, techListContent);
                    TechnologyUIItem itemScript = itemObj.GetComponent<TechnologyUIItem>();
                    if (itemScript != null)
                    {
                        itemScript.Setup(tech, this);
                    }
                }
            }
        }

        public void OnTechUnlockRequest(string techID)
        {
            ResearchManager.Instance.UnlockTechnology(techID);
            RefreshTechList(); // Refresh to show "Unlocked" state
        }
    }
}
