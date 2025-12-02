using NUnit.Framework;
using UnityEngine;
using SpaceRush.Systems;
using SpaceRush.Models;
using SpaceRush.Core;
using System.Collections.Generic;

namespace SpaceRush.Tests
{
    public class ProcessingTests
    {
        private GameObject gameObj;

        [SetUp]
        public void Setup()
        {
            gameObj = new GameObject("GameManager");
            gameObj.AddComponent<GameDatabase>();
            // Force load by calling InitializeDatabase via reflection (it's private) or just let Awake handle it
            // Awake is called by AddComponent in UnityMocks

            gameObj.AddComponent<ResearchManager>();
            gameObj.AddComponent<ProcessingSystem>();
            gameObj.AddComponent<LocationManager>();

            // Re-initialize tech tree because ResearchManager.Start might not have run if we didn't use AddComponent properly or if dependencies weren't ready
            ResearchManager.Instance.SendMessage("Start");
        }

        [TearDown]
        public void Teardown()
        {
            UnityEngine.Object.DestroyImmediate(gameObj);
        }

        [Test]
        public void Test_RecipeLoading()
        {
            var recipe = GameDatabase.Instance.GetRecipe("SMELT_STEEL");
            Assert.IsNotNull(recipe);
            Assert.AreEqual("Steel Smelting", recipe.Name);
            Assert.AreEqual(ResourceType.Iron, recipe.InputResource);
        }

        [Test]
        public void Test_ExecuteProcessing()
        {
            // Setup Location
            var loc = new LocationState("TEST_LOC");
            loc.Stockpile[ResourceType.Iron] = 100;
            loc.Infrastructure.ProcessingLevel = 5; // 5 / 5s = 1 op/sec
            loc.ActiveRecipeID = "SMELT_STEEL";

            // Setup Tech
            // SMELT_STEEL requires EFFICIENCY_1
            var rm = ResearchManager.Instance;
            var tech = rm.GetAllTechnologies().Find(t => t.ID == "EFFICIENCY_1");
            if (tech != null) tech.IsUnlocked = true;

            // Act
            ProcessingSystem.Instance.ProcessLocation(loc);

            // Assert
            // Recipe: 5 Iron -> 1 Steel.
            // 1 Op performed.
            Assert.AreEqual(95, loc.Stockpile[ResourceType.Iron]);
            Assert.IsTrue(loc.Stockpile.ContainsKey(ResourceType.Steel));
            Assert.AreEqual(1, loc.Stockpile[ResourceType.Steel]);
        }
    }
}
