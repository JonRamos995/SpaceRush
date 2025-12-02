using UnityEngine;
using SpaceRush.Core;
using SpaceRush.Models;
using System.Collections;
using System.Collections.Generic;

namespace SpaceRush.Systems
{
    public class WorkshopManager : MonoBehaviour
    {
        public static WorkshopManager Instance { get; private set; }

        public const float AI_INSTALLATION_COST = 2000f;

        public List<WorkshopSlot> Slots { get; private set; } = new List<WorkshopSlot>();
        private int unlockedSlots = 1;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeWorkshop();
            }
            else Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void InitializeWorkshop()
        {
            Slots.Clear();
            // Default: 1 Slot with Basic Smelter
            Slots.Add(new WorkshopSlot
            {
                SlotIndex = 0,
                InstalledMachine = MachineType.BasicSmelter,
                IsAutomated = false
            });
            unlockedSlots = 1;
        }

        private void Start()
        {
            StartCoroutine(WorkshopLoop());
        }

        private IEnumerator WorkshopLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(1.0f);
                TickWorkshop();
            }
        }

        private void TickWorkshop()
        {
            RefreshUnlocks();

            foreach (var slot in Slots)
            {
                if (slot.InstalledMachine == MachineType.None) continue;

                // Handle active jobs
                if (slot.IsWorking)
                {
                     AdvanceJob(slot);
                }
                // Handle Automation (Auto-Restart)
                else if (slot.IsAutomated && !string.IsNullOrEmpty(slot.ActiveRecipeID))
                {
                    StartJob(slot.SlotIndex, slot.ActiveRecipeID);
                }
            }
        }

        public void StartJob(int slotIndex, string recipeID)
        {
            if (slotIndex < 0 || slotIndex >= Slots.Count) return;
            var slot = Slots[slotIndex];

            if (slot.IsWorking) return; // Already working

            RecipeDefinition recipe = GameDatabase.Instance.GetRecipe(recipeID);
            if (recipe == null) return;

            // Check if Machine Type supports this recipe?
            // For MVP, we assume BasicSmelter handles "Smelting" recipes.
            // Ideally RecipeDefinition has "RequiredMachine".
            // Let's assume ANY machine works for now, or check ID prefix?
            // User said: "smelt raw metals... forge them".
            // Let's enforce strictly via Recipe ID prefix or assume player is smart.

            // Check Inputs (Global Inventory)
            if (ResourceManager.Instance.GetResourceQuantity(recipe.InputResource) >= recipe.InputAmount)
            {
                // Consume Input
                ResourceManager.Instance.RemoveResource(recipe.InputResource, recipe.InputAmount);

                // Set State
                slot.ActiveRecipeID = recipeID;
                slot.IsWorking = true;
                slot.Progress = 0f;
                GameLogger.Log($"Started crafting {recipe.Name} in Slot {slotIndex}");
            }
            else
            {
                if (!slot.IsAutomated) GameLogger.Log("Not enough resources to start job.");
            }
        }

        private void AdvanceJob(WorkshopSlot slot)
        {
             RecipeDefinition recipe = GameDatabase.Instance.GetRecipe(slot.ActiveRecipeID);
             if (recipe == null)
             {
                 slot.IsWorking = false;
                 return;
             }

             // Calculate Speed (could be based on Machine Type or Upgrades)
             float duration = Mathf.Max(0.1f, (float)recipe.DurationSeconds);
             float progressPerTick = 1.0f / duration;

             // Apply Ship Level Bonus? "Tiers available here depend on advancements and ship level"
             // Maybe speed is just fixed for now.

             slot.Progress += progressPerTick;

             if (slot.Progress >= 1.0f)
             {
                 CompleteJob(slot, recipe);
             }
        }

        private void CompleteJob(WorkshopSlot slot, RecipeDefinition recipe)
        {
            ResourceManager.Instance.AddResource(recipe.OutputResource, recipe.OutputAmount);
            GameLogger.Log($"Crafted {recipe.OutputAmount} {recipe.Name}");

            slot.IsWorking = false;
            slot.Progress = 0f;

            // If manual, it stops here. If automated, TickWorkshop will restart it next tick.
        }

        public void InstallMachine(int slotIndex, MachineType type)
        {
            if (slotIndex < 0 || slotIndex >= Slots.Count) return;
            Slots[slotIndex].InstalledMachine = type;
        }

        public void InstallAI(int slotIndex)
        {
             if (slotIndex < 0 || slotIndex >= Slots.Count) return;

             float cost = AI_INSTALLATION_COST;
             if (ResourceManager.Instance.SpendCredits(cost))
             {
                 Slots[slotIndex].IsAutomated = true;
                 GameLogger.Log($"AI installed in Slot {slotIndex}. Automation Enabled.");
             }
             else
             {
                 GameLogger.Log($"Not enough credits to install AI. Cost: {cost}");
             }
        }

        private void RefreshUnlocks()
        {
            if (FleetManager.Instance == null) return;

            // Unlock slots based on Ship Level (1 slot per level)
            int desiredSlots = FleetManager.Instance.ShipLevel;

            if (desiredSlots > unlockedSlots)
            {
                for (int i = unlockedSlots; i < desiredSlots; i++)
                {
                    Slots.Add(new WorkshopSlot
                    {
                        SlotIndex = i,
                        InstalledMachine = MachineType.BasicSmelter, // Default to Smelter for now
                        IsAutomated = false
                    });
                }
                unlockedSlots = desiredSlots;
                GameLogger.Log($"Workshop Expanded: Now have {unlockedSlots} slots.");
            }
        }

        // --- Persistence ---

        public void ResetData()
        {
            InitializeWorkshop();
        }

        public void LoadData(WorkshopSaveData data)
        {
            if (data == null) return;
            unlockedSlots = data.UnlockedSlots;
            Slots = data.Slots ?? new List<WorkshopSlot>();

            // Validate Slots
            if (Slots.Count == 0) InitializeWorkshop();
        }

        public WorkshopSaveData GetSaveData()
        {
            return new WorkshopSaveData
            {
                UnlockedSlots = unlockedSlots,
                Slots = new List<WorkshopSlot>(Slots)
            };
        }
    }
}
