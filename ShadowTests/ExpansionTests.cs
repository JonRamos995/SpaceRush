using NUnit.Framework;
using UnityEngine;
using SpaceRush.Core;
using SpaceRush.Systems;
using SpaceRush.Models;
using System.Linq;

namespace ShadowTests
{
    [TestFixture]
    public class ExpansionTests
    {
        private GameManager gameManager;
        private ResourceManager resourceManager;
        private CivilizationManager civilizationManager;

        [SetUp]
        public void SetUp()
        {
            // Reset Singletons
            if (GameManager.Instance != null) Object.DestroyImmediate(GameManager.Instance.gameObject);
            if (GameDatabase.Instance != null) Object.DestroyImmediate(GameDatabase.Instance.gameObject);

            // Ensure Database is initialized first
            var dbGo = new GameObject("GameDatabase");
            dbGo.AddComponent<GameDatabase>();

            var go = new GameObject("GameManager");
            gameManager = go.AddComponent<GameManager>();

            // Trigger Start to initialize sub-systems
            var startMethod = gameManager.GetType().GetMethod("Start", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (startMethod != null) startMethod.Invoke(gameManager, null);

            resourceManager = ResourceManager.Instance;
            civilizationManager = CivilizationManager.Instance;
        }

        [TearDown]
        public void TearDown()
        {
            if (GameManager.Instance != null) Object.DestroyImmediate(GameManager.Instance.gameObject);
            if (GameDatabase.Instance != null) Object.DestroyImmediate(GameDatabase.Instance.gameObject);
        }

        [Test]
        public void VerifyNewResources()
        {
            var titanium = resourceManager.GetResourceData(ResourceType.Titanium);
            Assert.IsNotNull(titanium, "Titanium should exist");
            Assert.AreEqual(80f, titanium.BaseValue);

            var helium3 = resourceManager.GetResourceData(ResourceType.Helium3);
            Assert.IsNotNull(helium3, "Helium3 should exist");

            var cryo = resourceManager.GetResourceData(ResourceType.CryoFluid);
            Assert.IsNotNull(cryo, "CryoFluid should exist");
        }

        [Test]
        public void VerifyNewLocations()
        {
            // Manually re-trigger InitializeLocations because GameDatabase might not have been fully ready when LocationManager Awake ran?
            // Or LocationManager Start ran but GameDatabase instance wasn't set?
            // In SetUp we create GameDatabase BEFORE GameManager.
            // GameManager creates LocationManager. LocationManager Start calls Init.
            // Should be fine.

            // Wait, LocationManager relies on Start.
            // GameManager.Start creates LocationManager via AddComponent.
            // AddComponent calls Awake. It does NOT call Start immediately in Unity (usually next frame).
            // But my Mock `AddComponent` calls `Awake` immediately.
            // It does NOT call `Start`.
            // So LocationManager.Start is never called!

            // I need to manually invoke Start on LocationManager or call InitializeLocations.
            var locMgr = LocationManager.Instance;
            var method = locMgr.GetType().GetMethod("InitializeLocations", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (method != null) method.Invoke(locMgr, null);

            var locs = LocationManager.Instance.Locations;

            var mercury = locs.Find(l => l.ID == "MERCURY");
            Assert.IsNotNull(mercury, "Mercury missing");
            Assert.AreEqual(2, (int)mercury.Definition.Biome); // Volcanic
            Assert.IsTrue(mercury.Definition.AvailableResources.Contains(ResourceType.Titanium));

            var saturn = locs.Find(l => l.ID == "SATURN");
            Assert.IsNotNull(saturn, "Saturn missing");
            Assert.AreEqual(4, (int)saturn.Definition.Biome); // GasGiant

            var titan = locs.Find(l => l.ID == "TITAN");
            Assert.IsNotNull(titan, "Titan missing");
            Assert.AreEqual(3, (int)titan.Definition.Biome); // Ice
        }

        [Test]
        public void VerifyCivilizationAscension()
        {
            // 1. Set up initial state
            resourceManager.AddCredits(1000f);
            Assert.AreEqual(1000f, resourceManager.Credits);

            Assert.AreEqual(0, civilizationManager.Level);

            // 2. Trigger Ascend
            civilizationManager.Ascend();

            // 3. Verify Civ Level increased
            Assert.AreEqual(1, civilizationManager.Level);
            Assert.AreEqual(1.1f, civilizationManager.Multiplier);

            // Verify Multiplier Effect on Fleet
            // Fleet was reset to Level 1 (Base 1.0). Civ Multiplier 1.1 -> Speed 1.1
            Assert.AreEqual(1.1f, FleetManager.Instance.MiningSpeed, 0.01f);

            // 4. Verify Reset (Credits should be 0)
            Assert.AreEqual(0f, resourceManager.Credits);

            // 5. Verify Persistence
            var saveData = civilizationManager.GetSaveData();
            Assert.AreEqual(1, saveData.Level);
        }
    }
}
