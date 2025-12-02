using NUnit.Framework;
using UnityEngine;
using SpaceRush.Systems;
using SpaceRush.Models;
using SpaceRush.Core;
using SpaceRush.Data;

namespace SpaceRush.Tests
{
    public class ResearchTests
    {
        private GameObject gameGameObject;
        private ResearchManager researchManager;
        private ResourceManager resourceManager;

        [SetUp]
        public void Setup()
        {
            gameGameObject = new GameObject("GameManager");
            resourceManager = gameGameObject.AddComponent<ResourceManager>();

            // We need GameDatabase for ResearchManager to initialize
            var dbGo = new GameObject("Database");
            var db = dbGo.AddComponent<GameDatabase>();
            // Force Awake (Reflectively or just rely on Monobehaviour lifecycle in PlayMode, but here we might need manual init)
            // Since we can't easily force Awake in simple unit tests without PlayMode,
            // and GameDatabase populates in Awake, we need to be careful.
            // For this test, we might need to manually populate GameDatabase or use reflection to call Awake.
            // Or just call the method if we make it public. Awake is private.
            // We can use SendMessage or similar.
            // db.SendMessage("Awake"); // Removed: AddComponent calls Awake automatically in Mock

            researchManager = gameGameObject.AddComponent<ResearchManager>();
            // researchManager.SendMessage("Awake"); // Removed: AddComponent calls Awake automatically in Mock
            researchManager.SendMessage("Start");

            // FleetManager (required for tech effects)
            gameGameObject.AddComponent<FleetManager>();
        }

        [TearDown]
        public void Teardown()
        {
            if (GameDatabase.Instance != null) UnityEngine.Object.DestroyImmediate(GameDatabase.Instance.gameObject);
            UnityEngine.Object.DestroyImmediate(gameGameObject);
        }

        [Test]
        public void TestHireResearcher()
        {
            resourceManager.AddCredits(2000);
            researchManager.HireResearcher();
            Assert.AreEqual(1, researchManager.Researchers);
        }

        [Test]
        public void TestUnlockTech_Success()
        {
            // Pick a cheap tech
            string techID = "EFFICIENCY_1"; // Cost 100 RP

            // Grant Credits first
            resourceManager.AddCredits(1000);

            // Grant RP
            researchManager.InvestInResearch(1000); // Should give 100 RP

            // Attempt Unlock
            researchManager.UnlockTechnology(techID);

            Assert.IsTrue(researchManager.IsTechUnlocked(techID));
        }

        [Test]
        public void TestUnlockTech_NotEnoughRP()
        {
            string techID = "EFFICIENCY_1";
            // 0 RP

            researchManager.UnlockTechnology(techID);
            Assert.IsFalse(researchManager.IsTechUnlocked(techID));
        }
    }
}
