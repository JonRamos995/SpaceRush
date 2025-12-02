using UnityEngine;
using SpaceRush.Data;
using SpaceRush.Systems; // For BiomeType
using SpaceRush.Models;
using System.Collections.Generic;

namespace SpaceRush.Core
{
    /// <summary>
    /// Acts as a central repository for all static game data.
    /// In a full production pipeline, this would load Assets from Resources/Addressables.
    /// For this prototype, we populate it manually in code to avoid Editor dependency issues.
    /// </summary>
    public class GameDatabase : MonoBehaviour
    {
        public static GameDatabase Instance { get; private set; }

        public List<LocationDefinition> Locations { get; private set; }
        public List<TechDefinition> Technologies { get; private set; }
        public List<RecipeDefinition> Recipes { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeDatabase();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void InitializeDatabase()
        {
            Locations = new List<LocationDefinition>();
            Technologies = new List<TechDefinition>();
            Recipes = new List<RecipeDefinition>();

            LoadTechnologies();
            LoadLocations();
            LoadRecipes();
        }

        private void LoadRecipes()
        {
             TextAsset file = Resources.Load<TextAsset>("Data/recipes");
            if (file != null)
            {
                RecipeDataWrapper wrapper = JsonUtility.FromJson<RecipeDataWrapper>(file.text);
                if (wrapper != null && wrapper.Items != null)
                {
                    Recipes = wrapper.Items;
                    GameLogger.Log($"Loaded {Recipes.Count} recipes from JSON.");
                }
            }
            else
            {
                GameLogger.LogError("Could not load Data/recipes.json");
            }
        }

        private void LoadTechnologies()
        {
            TextAsset file = Resources.Load<TextAsset>("Data/technologies");
            if (file != null)
            {
                TechDataWrapper wrapper = JsonUtility.FromJson<TechDataWrapper>(file.text);
                if (wrapper != null && wrapper.Items != null)
                {
                    Technologies = wrapper.Items;
                    GameLogger.Log($"Loaded {Technologies.Count} technologies from JSON.");
                }
            }
            else
            {
                GameLogger.LogError("Could not load Data/technologies.json");
            }
        }

        private void LoadLocations()
        {
            TextAsset file = Resources.Load<TextAsset>("Data/locations");
            if (file != null)
            {
                LocationDataWrapper wrapper = JsonUtility.FromJson<LocationDataWrapper>(file.text);
                if (wrapper != null && wrapper.Items != null)
                {
                    Locations = wrapper.Items;
                    GameLogger.Log($"Loaded {Locations.Count} locations from JSON.");
                }
            }
            else
            {
                GameLogger.LogError("Could not load Data/locations.json");
            }
        }

        public LocationDefinition GetLocation(string id)
        {
            return Locations.Find(l => l.ID == id);
        }

        public TechDefinition GetTech(string id)
        {
            return Technologies.Find(t => t.ID == id);
        }

        public RecipeDefinition GetRecipe(string id)
        {
            return Recipes.Find(r => r.ID == id);
        }
    }
}
