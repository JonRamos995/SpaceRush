using UnityEngine;
using SpaceRush.Core;
using SpaceRush.Models;
using System.Collections;

namespace SpaceRush.Systems
{
    public class ProcessingSystem : MonoBehaviour
    {
        public static ProcessingSystem Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            StartCoroutine(ProcessingLoop());
        }

        private IEnumerator ProcessingLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(1.0f);
                ProcessAllLocations();
            }
        }

        public void ProcessAllLocations()
        {
            if (LocationManager.Instance == null) return;

            foreach (var loc in LocationManager.Instance.Locations)
            {
                ProcessLocation(loc);
            }
        }

        public void ProcessLocation(LocationState loc)
        {
            if (loc.Infrastructure.ProcessingLevel <= 0 || string.IsNullOrEmpty(loc.ActiveRecipeID))
                return;

            RecipeDefinition recipe = GameDatabase.Instance.GetRecipe(loc.ActiveRecipeID);
            if (recipe == null) return;

            // Check tech
            if (!string.IsNullOrEmpty(recipe.RequiredTechID) && !ResearchManager.Instance.IsTechUnlocked(recipe.RequiredTechID))
                return;

            // Calculate Throughput
            // Operations per tick (1s) = (Level * 1.0) / Duration
            float opsPerSecond = loc.Infrastructure.ProcessingLevel / Mathf.Max(0.1f, recipe.DurationSeconds);

            int potentialOps = Mathf.FloorToInt(opsPerSecond);
            if (potentialOps == 0)
            {
                // Handle slow recipes (duration > 1s) via probability
                if (Random.Range(0f, 1f) < opsPerSecond) potentialOps = 1;
            }

            if (potentialOps > 0)
            {
                 ExecuteRecipe(loc, recipe, potentialOps);
            }
        }

        private void ExecuteRecipe(LocationState loc, RecipeDefinition recipe, int times)
        {
            // Check stockpile for inputs
            if (!loc.Stockpile.ContainsKey(recipe.InputResource)) return;

            int availableInput = loc.Stockpile[recipe.InputResource];
            int requiredInputTotal = recipe.InputAmount * times;

            // Cap times by availability
            if (availableInput < requiredInputTotal)
            {
                times = availableInput / recipe.InputAmount;
            }

            if (times > 0)
            {
                // Consume
                loc.Stockpile[recipe.InputResource] -= recipe.InputAmount * times;

                // Produce
                if (!loc.Stockpile.ContainsKey(recipe.OutputResource))
                    loc.Stockpile[recipe.OutputResource] = 0;

                loc.Stockpile[recipe.OutputResource] += recipe.OutputAmount * times;
            }
        }
    }
}
