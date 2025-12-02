using System.Collections.Generic;
using NUnit.Framework;
using SpaceRush.Core;
using SpaceRush.Systems;
using SpaceRush.Models;
using SpaceRush.Data;
using UnityEngine;
using System.Linq;

namespace ShadowTests
{
    [TestFixture]
    public class NewContentTests
    {
        private GameManager gameManager;
        private ResourceManager resourceManager;
        private LogisticsSystem logisticsSystem;

        [SetUp]
        public void Setup()
        {
            // Reset Singletons
            if (GameManager.Instance != null) Object.DestroyImmediate(GameManager.Instance.gameObject);
            if (GameDatabase.Instance != null) Object.DestroyImmediate(GameDatabase.Instance.gameObject);

            // Ensure Database is initialized first
            var dbGo = new GameObject("GameDatabase");
            dbGo.AddComponent<GameDatabase>();

            GameObject go = new GameObject("GameManager");
            gameManager = go.AddComponent<GameManager>();

            // Invoke Start to initialize systems
            var startMethod = typeof(GameManager).GetMethod("Start", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (startMethod != null) startMethod.Invoke(gameManager, null);

            resourceManager = ResourceManager.Instance;
            logisticsSystem = LogisticsSystem.Instance;
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
            // Hydrogen, Steel, Circuit should be present
            var all = resourceManager.GetAllResources();
            bool hasHydrogen = all.Any(r => r.Type == ResourceType.Hydrogen);
            bool hasSteel = all.Any(r => r.Type == ResourceType.Steel);
            bool hasCircuit = all.Any(r => r.Type == ResourceType.Circuit);

            Assert.IsTrue(hasHydrogen, "Hydrogen missing");
            Assert.IsTrue(hasSteel, "Steel missing");
            Assert.IsTrue(hasCircuit, "Circuit missing");
        }

        [Test]
        public void VerifyJupiterLocation()
        {
            // Reload database to ensure JSON is picked up
            // GameDatabase loads in Awake.

            // Wait, LocationManager initializes locations in Start/Awake.
            // Since we created GameManager, it creates LocationManager.
            // But LocationManager relies on GameDatabase.

            // Let's force LocationManager to initialize if it hasn't properly.
            // LocationManager uses Start() to InitializeLocations.
            // Mock GameObject.AddComponent calls Awake, but NOT Start.
            // I need to manually call Start logic or simulate it.
            // But LocationManager is private...
            // Wait, in UnityMocks, AddComponent calls Awake.
            // LocationManager calls InitializeLocations in Start.

            // So Locations might be empty.
            // I can use reflection to call Start or InitializeLocations.

            var locMgr = LocationManager.Instance;
            var method = locMgr.GetType().GetMethod("InitializeLocations", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (method != null) method.Invoke(locMgr, null);

            var locs = LocationManager.Instance.Locations;
            var jupiter = locs.Find(l => l.ID == "JUPITER");

            Assert.IsNotNull(jupiter, "Jupiter not found in LocationManager");
            Assert.AreEqual(4, (int)jupiter.Definition.Biome); // GasGiant (4)
            Assert.IsTrue(jupiter.Definition.AvailableResources.Contains(ResourceType.Hydrogen));
        }

        [Test]
        public void VerifyLogisticsAllocation()
        {
            logisticsSystem.SetAllocation(ResourceType.Hydrogen, 0.5f);
            Assert.IsTrue(logisticsSystem.CargoAllocations.ContainsKey(ResourceType.Hydrogen));
            Assert.AreEqual(0.5f, logisticsSystem.CargoAllocations[ResourceType.Hydrogen]);
        }
    }
}
