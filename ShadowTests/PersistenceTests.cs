using NUnit.Framework;
using UnityEngine;
using SpaceRush.Core;
using SpaceRush.Systems;
using SpaceRush.Models;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace ShadowTests
{
    [TestFixture]
    public class PersistenceTests
    {
        private PersistenceManager persistenceManager;
        private LogisticsSystem logisticsSystem;
        private string saveFile = "savegame.json";

        [SetUp]
        public void Setup()
        {
            // Clean up old saves
            if (File.Exists(saveFile)) File.Delete(saveFile);

            // Reset Singletons
            if (GameManager.Instance != null) Object.DestroyImmediate(GameManager.Instance.gameObject);
            if (GameDatabase.Instance != null) Object.DestroyImmediate(GameDatabase.Instance.gameObject);
            if (ResearchManager.Instance != null) Object.DestroyImmediate(ResearchManager.Instance.gameObject);
            if (FleetManager.Instance != null) Object.DestroyImmediate(FleetManager.Instance.gameObject);
            if (LocationManager.Instance != null) Object.DestroyImmediate(LocationManager.Instance.gameObject);
            if (ResourceManager.Instance != null) Object.DestroyImmediate(ResourceManager.Instance.gameObject);
            if (LogisticsSystem.Instance != null) Object.DestroyImmediate(LogisticsSystem.Instance.gameObject);
            if (WorkshopManager.Instance != null) Object.DestroyImmediate(WorkshopManager.Instance.gameObject);

            // 1. Setup Database
            var dbGo = new GameObject("GameDatabase");
            dbGo.AddComponent<GameDatabase>();

            // 2. Setup Systems Manually to ensure they exist and are initialized
            GameObject systemsGo = new GameObject("Systems");

            // Core
            systemsGo.AddComponent<ResourceManager>();
            var rm = systemsGo.AddComponent<ResearchManager>();
            // Initialize Research Tech Tree
            MethodInfo rmStart = rm.GetType().GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic);
            if (rmStart != null) rmStart.Invoke(rm, null);

            systemsGo.AddComponent<FleetManager>();

            var lm = systemsGo.AddComponent<LocationManager>();
            // Initialize Locations
             MethodInfo lmStart = lm.GetType().GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic);
            if (lmStart != null) lmStart.Invoke(lm, null);

            logisticsSystem = systemsGo.AddComponent<LogisticsSystem>();
            systemsGo.AddComponent<WorkshopManager>();

            // 3. Persistence Manager
            persistenceManager = systemsGo.AddComponent<PersistenceManager>();
            systemsGo.AddComponent<IdleManager>();

            // 4. GameManager (Optional for this test but good for completeness)
            GameObject gmGo = new GameObject("GameManager");
            gmGo.AddComponent<GameManager>();
        }

        [TearDown]
        public void TearDown()
        {
            if (GameManager.Instance != null) Object.DestroyImmediate(GameManager.Instance.gameObject);
            if (GameDatabase.Instance != null) Object.DestroyImmediate(GameDatabase.Instance.gameObject);
            // Destroy systems go? Since they are DontDestroyOnLoad, they persist.
            if (PersistenceManager.Instance != null) Object.DestroyImmediate(PersistenceManager.Instance.gameObject);

            if (File.Exists(saveFile)) File.Delete(saveFile);
        }

        [Test]
        public void VerifyLogisticsPersistence()
        {
            // 1. Set Allocation
            LogisticsSystem.Instance.SetAllocation(ResourceType.Iron, 0.5f);
            LogisticsSystem.Instance.SetAllocation(ResourceType.Gold, 0.2f);

            Assert.AreEqual(0.5f, LogisticsSystem.Instance.CargoAllocations[ResourceType.Iron]);

            // 2. Save
            PersistenceManager.Instance.SaveGame();

            Assert.IsTrue(File.Exists(saveFile), "Save file was not created.");

            // 3. Clear State (simulate restart)
            LogisticsSystem.Instance.CargoAllocations.Clear();
            Assert.AreEqual(0, LogisticsSystem.Instance.CargoAllocations.Count);

            // 4. Load
            bool loaded = PersistenceManager.Instance.LoadGame();
            Assert.IsTrue(loaded, "Failed to load game.");

            // 5. Verify
            Assert.IsTrue(LogisticsSystem.Instance.CargoAllocations.ContainsKey(ResourceType.Iron), "Iron allocation missing.");
            Assert.AreEqual(0.5f, LogisticsSystem.Instance.CargoAllocations[ResourceType.Iron], "Iron pct mismatch.");

            Assert.IsTrue(LogisticsSystem.Instance.CargoAllocations.ContainsKey(ResourceType.Gold), "Gold allocation missing.");
            Assert.AreEqual(0.2f, LogisticsSystem.Instance.CargoAllocations[ResourceType.Gold], "Gold pct mismatch.");
        }

        [Test]
        public void VerifyWorkshopPersistence()
        {
            // 1. Modify Workshop State
            WorkshopManager.Instance.UnlockSmelter(true); // Should be 2 smelters total
            WorkshopManager.Instance.UnlockAssembler(true); // Should be 2 assemblers total

            Assert.AreEqual(2, WorkshopManager.Instance.SmelterCount);
            Assert.AreEqual(2, WorkshopManager.Instance.AssemblerCount);

            // 2. Save
            PersistenceManager.Instance.SaveGame();

            // 3. Clear/Corrupt State
            WorkshopManager.Instance.ResetData(); // Resets to 1 Smelter, 1 Assembler
            Assert.AreEqual(1, WorkshopManager.Instance.SmelterCount);

            // 4. Load
            PersistenceManager.Instance.LoadGame();

            // 5. Verify
            Assert.AreEqual(2, WorkshopManager.Instance.SmelterCount, "Smelter count not restored");
            Assert.AreEqual(2, WorkshopManager.Instance.AssemblerCount, "Assembler count not restored");
            Assert.AreEqual(4, WorkshopManager.Instance.Slots.Count, "Slots not restored");
        }
    }
}
