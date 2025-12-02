using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SpaceRush.Systems;
using SpaceRush.Models;
using SpaceRush.Core;
using System.Collections.Generic;

namespace SpaceRush.UI
{
    public class WorkshopSlotUI : MonoBehaviour
    {
        [Header("UI Components")]
        public TMP_Text MachineNameText;
        public TMP_Dropdown RecipeDropdown;
        public Image ProgressBar;
        public Button StartButton;
        public Button InstallAIButton;
        public TMP_Text InstallAIText;
        public TMP_Text StatusText;

        private int slotIndex;
        private WorkshopUIController controller;
        private List<string> availableRecipeIDs = new List<string>();

        public void Setup(int index, WorkshopUIController ctrl)
        {
            slotIndex = index;
            controller = ctrl;

            if (StartButton != null) StartButton.onClick.AddListener(OnStartClicked);
            if (InstallAIButton != null) InstallAIButton.onClick.AddListener(OnInstallAIClicked);

            RefreshRecipeList();
        }

        private void OnEnable()
        {
            RefreshRecipeList();
        }

        public void RefreshRecipeList()
        {
            if (RecipeDropdown == null) return;

            // Save current selection if possible?
            // For simplicity, reset to 0 or try to keep ID.

            RecipeDropdown.ClearOptions();
            availableRecipeIDs.Clear();

            if (GameDatabase.Instance == null) return;

            var allRecipes = GameDatabase.Instance.Recipes;
            List<string> options = new List<string>();

            foreach (var r in allRecipes)
            {
                // Check Tech Requirement
                bool unlocked = string.IsNullOrEmpty(r.RequiredTechID) ||
                                (ResearchManager.Instance != null && ResearchManager.Instance.IsTechUnlocked(r.RequiredTechID));

                if (unlocked)
                {
                    availableRecipeIDs.Add(r.ID);
                    options.Add($"{r.Name} ({r.InputAmount} {r.InputResource})");
                }
            }

            RecipeDropdown.AddOptions(options);
        }

        public void UpdateUI()
        {
            if (WorkshopManager.Instance == null || slotIndex >= WorkshopManager.Instance.Slots.Count) return;

            var slot = WorkshopManager.Instance.Slots[slotIndex];

            // Update Machine Name
            if (MachineNameText != null)
                MachineNameText.text = $"{slot.InstalledMachine} (Slot {slotIndex + 1})";

            // Update Progress
            if (ProgressBar != null)
                ProgressBar.fillAmount = slot.Progress;

            // Update Buttons
            bool isBusy = slot.IsWorking;
            bool hasRecipes = availableRecipeIDs.Count > 0;
            bool canAffordRecipe = false;

            // Check Recipe Cost
            if (hasRecipes && RecipeDropdown != null && RecipeDropdown.value >= 0 && RecipeDropdown.value < availableRecipeIDs.Count)
            {
                string rID = availableRecipeIDs[RecipeDropdown.value];
                if (GameDatabase.Instance != null)
                {
                    var recipe = GameDatabase.Instance.GetRecipe(rID);
                    if (recipe != null)
                    {
                        canAffordRecipe = ResourceManager.Instance.GetResourceQuantity(recipe.InputResource) >= recipe.InputAmount;
                    }
                }
            }

            if (StartButton != null) StartButton.interactable = !isBusy && hasRecipes && canAffordRecipe;
            if (RecipeDropdown != null) RecipeDropdown.interactable = !isBusy && !slot.IsAutomated && hasRecipes;

            // AI Status
            if (slot.IsAutomated)
            {
                if (InstallAIButton != null) InstallAIButton.gameObject.SetActive(false);
                if (StatusText != null) StatusText.text = isBusy ? "AUTO-WORKING" : "AUTO-IDLE";
            }
            else
            {
                if (InstallAIButton != null)
                {
                    InstallAIButton.gameObject.SetActive(true);
                    float aiCost = WorkshopManager.AI_INSTALLATION_COST;
                    bool canAffordAI = ResourceManager.Instance != null && ResourceManager.Instance.Credits >= aiCost;
                    InstallAIButton.interactable = canAffordAI;

                    if (InstallAIText != null) InstallAIText.text = $"Install AI ({aiCost} CR)";
                }
                if (StatusText != null) StatusText.text = isBusy ? "WORKING" : "IDLE";
            }
        }

        private void OnStartClicked()
        {
            if (availableRecipeIDs.Count == 0) return;
            int selected = RecipeDropdown.value;
            if (selected >= 0 && selected < availableRecipeIDs.Count)
            {
                string recipeID = availableRecipeIDs[selected];
                WorkshopManager.Instance.StartJob(slotIndex, recipeID);
            }
        }

        private void OnInstallAIClicked()
        {
            WorkshopManager.Instance.InstallAI(slotIndex);
        }
    }
}
