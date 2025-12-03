using UnityEngine;
using SpaceRush.Core;
using SpaceRush.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SpaceRush.Systems
{
    public class WorkshopManager : MonoBehaviour
    {
        public static WorkshopManager Instance { get; private set; }

        public const float AI_INSTALLATION_COST = 2000f;
        public const int MAX_MACHINES_PER_TYPE = 5;

        public List<WorkshopSlot> Slots { get; private set; } = new List<WorkshopSlot>();

        public int SmelterCount { get; private set; }
        public int AssemblerCount { get; private set; }

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
            SmelterCount = 0;
            AssemblerCount = 0;

            // Start with 1 Smelter and 1 Assembler
            UnlockSmelter(true);
            UnlockAssembler(true);
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

        public void UnlockSmelter(bool free = false)
        {
            if (SmelterCount >= MAX_MACHINES_PER_TYPE)
            {
                GameLogger.Log("Max Smelters reached.");
                return;
            }

            Slots.Add(new WorkshopSlot
            {
                SlotIndex = Slots.Count,
                InstalledMachine = MachineType.BasicSmelter,
                IsAutomated = false
            });
            SmelterCount++;
            SortSlots(); // Keep sorted

            if (!free) GameLogger.Log($"Unlocked Smelter. Total: {SmelterCount}");
        }

        public void UnlockAssembler(bool free = false)
        {
            if (AssemblerCount >= MAX_MACHINES_PER_TYPE)
            {
                GameLogger.Log("Max Assemblers reached.");
                return;
            }

            Slots.Add(new WorkshopSlot
            {
                SlotIndex = Slots.Count,
                InstalledMachine = MachineType.Assembler,
                IsAutomated = false
            });
            AssemblerCount++;
            SortSlots(); // Keep sorted

            if (!free) GameLogger.Log($"Unlocked Assembler. Total: {AssemblerCount}");
        }

        private void SortSlots()
        {
            // Sort by Machine Type (Smelter=1, Assembler=2)
            Slots.Sort((a, b) => a.InstalledMachine.CompareTo(b.InstalledMachine));

            // Re-index
            for (int i = 0; i < Slots.Count; i++)
            {
                Slots[i].SlotIndex = i;
            }
        }

        public void StartJob(int slotIndex, string recipeID)
        {
            if (slotIndex < 0 || slotIndex >= Slots.Count) return;
            var slot = Slots[slotIndex];

            if (slot.IsWorking) return;

            RecipeDefinition recipe = GameDatabase.Instance.GetRecipe(recipeID);
            if (recipe == null) return;

            // Check Machine Requirement
            if (slot.InstalledMachine != recipe.RequiredMachine)
            {
                if (!slot.IsAutomated)
                    GameLogger.Log($"Incorrect machine. Recipe requires {recipe.RequiredMachine}, slot has {slot.InstalledMachine}.");
                return;
            }

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

             // Calculate Speed
             float duration = Mathf.Max(0.1f, (float)recipe.DurationSeconds);
             float progressPerTick = 1.0f / duration;

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

        // --- Persistence ---

        public void ResetData()
        {
            InitializeWorkshop();
        }

        public void LoadData(WorkshopSaveData data)
        {
            if (data == null) return;

            Slots = data.Slots ?? new List<WorkshopSlot>();
            SmelterCount = data.SmelterCount;
            AssemblerCount = data.AssemblerCount;

            // Fallback for old save data or corrupted data
            if (Slots.Count == 0)
            {
                InitializeWorkshop();
            }
            else
            {
                // Re-validate counts if they are 0 but slots exist
                if (SmelterCount == 0 && AssemblerCount == 0)
                {
                     SmelterCount = Slots.Count(s => s.InstalledMachine == MachineType.BasicSmelter);
                     AssemblerCount = Slots.Count(s => s.InstalledMachine == MachineType.Assembler);
                }

                // Ensure Sorted & Indexed correctly on Load
                SortSlots();
            }
        }

        public WorkshopSaveData GetSaveData()
        {
            return new WorkshopSaveData
            {
                SmelterCount = SmelterCount,
                AssemblerCount = AssemblerCount,
                Slots = new List<WorkshopSlot>(Slots)
            };
        }

        public List<RecipeDefinition> GetRecipesForMachine(MachineType machineType)
        {
            if (GameDatabase.Instance == null || GameDatabase.Instance.Recipes == null)
                return new List<RecipeDefinition>();

            return GameDatabase.Instance.Recipes
                .Where(r => r.RequiredMachine == machineType)
                .ToList();
        }
    }
}
