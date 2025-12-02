using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SpaceRush.Systems;

namespace SpaceRush.UI
{
    public class TechnologyUIItem : MonoBehaviour
    {
        public TMP_Text techNameText;
        public TMP_Text costText;
        public TMP_Text descriptionText;
        public Button unlockButton;
        public Image iconImage; // Optional
        public GameObject lockedOverlay; // Optional

        private string techID;
        private ResearchUIController controller;

        public void Setup(Technology tech, ResearchUIController parentController)
        {
            this.controller = parentController;
            this.techID = tech.ID;

            if (techNameText != null) techNameText.text = tech.Name;
            if (descriptionText != null) descriptionText.text = tech.Description;

            if (tech.IsUnlocked)
            {
                if (costText != null) costText.text = "UNLOCKED";
                if (unlockButton != null) unlockButton.interactable = false;
            }
            else
            {
                if (costText != null) costText.text = $"{tech.ResearchPointsRequired} RP";
                if (unlockButton != null)
                {
                    unlockButton.interactable = ResearchManager.Instance.ResearchPoints >= tech.ResearchPointsRequired;
                    unlockButton.onClick.RemoveAllListeners();
                    unlockButton.onClick.AddListener(OnUnlockClicked);
                }
            }
        }

        private void OnUnlockClicked()
        {
            controller.OnTechUnlockRequest(techID);
        }
    }
}
