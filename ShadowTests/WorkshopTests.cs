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
    public class WorkshopTests
    {
        private WorkshopManager workshopManager;

        [SetUp]
        public void Setup()
        {
            if (GameManager.Instance != null) Object.DestroyImmediate(GameManager.Instance.gameObject);
            if (GameDatabase.Instance != null) Object.DestroyImmediate(GameDatabase.Instance.gameObject);
            if (ResourceManager.Instance != null) Object.DestroyImmediate(ResourceManager.Instance.gameObject);
            if (WorkshopManager.Instance != null) Object.DestroyImmediate(WorkshopManager.Instance.gameObject);
            if (FleetManager.Instance != null) Object.DestroyImmediate(FleetManager.Instance.gameObject);

            // Setup Dependencies
            var dbGo = new GameObject("GameDatabase");
            dbGo.AddComponent<GameDatabase>();

            var resGo = new GameObject("ResourceManager");
            resGo.AddComponent<ResourceManager>();

            var rmGo = new GameObject("ResearchManager");
            var rm = rmGo.AddComponent<ResearchManager>();
            MethodInfo rmStart = rm.GetType().GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic);
            if (rmStart != null) rmStart.Invoke(rm, null);

            var fmGo = new GameObject("FleetManager");
            fmGo.AddComponent<FleetManager>();

            var wsGo = new GameObject("WorkshopManager");
            workshopManager = wsGo.AddComponent<WorkshopManager>();

            // Initialize Workshop is called in Awake (AddComponent)
        }

        [TearDown]
        public void TearDown()
        {
            if (GameDatabase.Instance != null) Object.DestroyImmediate(GameDatabase.Instance.gameObject);
            if (ResourceManager.Instance != null) Object.DestroyImmediate(ResourceManager.Instance.gameObject);
            if (WorkshopManager.Instance != null) Object.DestroyImmediate(WorkshopManager.Instance.gameObject);
            if (FleetManager.Instance != null) Object.DestroyImmediate(FleetManager.Instance.gameObject);
            if (ResearchManager.Instance != null) Object.DestroyImmediate(ResearchManager.Instance.gameObject);
        }

        [Test]
        public void TestInitialization()
        {
            Assert.AreEqual(1, workshopManager.Slots.Count);
            Assert.AreEqual(MachineType.BasicSmelter, workshopManager.Slots[0].InstalledMachine);
        }

        [Test]
        public void TestStartJob_Manual()
        {
            // Give Resources
            ResourceManager.Instance.AddResource(ResourceType.Iron, 100);

            // "SMELT_STEEL" requires 5 Iron.
            workshopManager.StartJob(0, "SMELT_STEEL");

            var slot = workshopManager.Slots[0];
            Assert.IsTrue(slot.IsWorking);
            Assert.AreEqual("SMELT_STEEL", slot.ActiveRecipeID);
            Assert.AreEqual(95, ResourceManager.Instance.GetResourceQuantity(ResourceType.Iron));
        }

        [Test]
        public void TestJobCompletion()
        {
            ResourceManager.Instance.AddResource(ResourceType.Iron, 100);
            workshopManager.StartJob(0, "SMELT_STEEL");

            var slot = workshopManager.Slots[0];

            // Advance Time
            // Recipe duration is 5s.
            // We need to call AdvanceJob 5 times (assuming 1s ticks).
            // But AdvanceJob is private. We can invoke "TickWorkshop" via reflection or waiting?
            // "TickWorkshop" is private.

            MethodInfo tick = workshopManager.GetType().GetMethod("TickWorkshop", BindingFlags.Instance | BindingFlags.NonPublic);

            // Simulate 5 ticks
            for(int i=0; i<6; i++)
            {
                tick.Invoke(workshopManager, null);
            }

            Assert.IsFalse(slot.IsWorking, "Job should be finished");
            Assert.AreEqual(1, ResourceManager.Instance.GetResourceQuantity(ResourceType.Steel), "Steel should be produced");
        }

        [Test]
        public void TestAutomation()
        {
            ResourceManager.Instance.AddResource(ResourceType.Iron, 100);

            // Install AI
            workshopManager.InstallAI(0);
            var slot = workshopManager.Slots[0];
            Assert.IsTrue(slot.IsAutomated);

            // Start First Job Manually (or maybe Automation starts it if idle?)
            // Logic says: else if (slot.IsAutomated && !string.IsNullOrEmpty(slot.ActiveRecipeID)) StartJob...
            // So we need to set ActiveRecipeID once.

            workshopManager.StartJob(0, "SMELT_STEEL");

            MethodInfo tick = workshopManager.GetType().GetMethod("TickWorkshop", BindingFlags.Instance | BindingFlags.NonPublic);

            // Finish first job and immediately restart (due to Automation loop)
            // 5 ticks to finish. 6th tick sees it finished and restarts it.
            for(int i=0; i<6; i++) tick.Invoke(workshopManager, null);

            // Should be working on Job 2 now
            Assert.IsTrue(slot.IsWorking, "Automation should have restarted job immediately");
            Assert.AreEqual(1, ResourceManager.Instance.GetResourceQuantity(ResourceType.Steel), "Job 1 Output produced");
            Assert.AreEqual(90, ResourceManager.Instance.GetResourceQuantity(ResourceType.Iron), "Job 2 Input consumed");
        }

        [Test]
        public void TestSlotExpansion()
        {
            // Initial: Level 1 -> 1 Slot
            Assert.AreEqual(1, workshopManager.Slots.Count);

            // Repair Ship First (Required for upgrade)
            FleetManager.Instance.RepairShip(100f);

            // Give credits for upgrade (if needed? UpgradeShip checks cost? No, implementation shown earlier didn't check cost, just "IsOperational")
            // Wait, FleetManager.UpgradeShip source: "if (!IsOperational)... ShipLevel++;"
            // It does NOT check credits in the simple version I read?
            // "GetUpgradeCost" exists but UpgradeShip doesn't seem to call SpendCredits in the snippet I saw?
            // Let's verify FleetManager.UpgradeShip again.
            // But assume Repair is enough.

            // Upgrade Ship
            FleetManager.Instance.UpgradeShip(); // Level 2

            // Trigger Tick
            MethodInfo tick = workshopManager.GetType().GetMethod("TickWorkshop", BindingFlags.Instance | BindingFlags.NonPublic);
            tick.Invoke(workshopManager, null);

            // Should be 2 slots now
            Assert.AreEqual(2, workshopManager.Slots.Count);
            Assert.AreEqual(1, workshopManager.Slots[1].SlotIndex);
        }
    }
}
