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
            // Should start with 1 Smelter and 1 Assembler
            Assert.AreEqual(2, workshopManager.Slots.Count);
            Assert.AreEqual(1, workshopManager.SmelterCount);
            Assert.AreEqual(1, workshopManager.AssemblerCount);

            Assert.AreEqual(MachineType.BasicSmelter, workshopManager.Slots[0].InstalledMachine);
            Assert.AreEqual(MachineType.Assembler, workshopManager.Slots[1].InstalledMachine);
        }

        [Test]
        public void TestStartJob_CorrectMachine()
        {
            ResourceManager.Instance.AddResource(ResourceType.Iron, 100);

            // SMELT_STEEL needs Smelter (Slot 0)
            workshopManager.StartJob(0, "SMELT_STEEL");

            var slot = workshopManager.Slots[0];
            Assert.IsTrue(slot.IsWorking);
            Assert.AreEqual("SMELT_STEEL", slot.ActiveRecipeID);
        }

        [Test]
        public void TestStartJob_WrongMachine()
        {
            ResourceManager.Instance.AddResource(ResourceType.Iron, 100);
            ResourceManager.Instance.AddResource(ResourceType.IronIngot, 100);

            // CRAFT_DRILL needs Assembler. Try starting in Slot 0 (Smelter).
            // NOTE: We need to ensure CRAFT_DRILL recipe is loaded and requires Assembler.

            workshopManager.StartJob(0, "CRAFT_DRILL");

            var slot = workshopManager.Slots[0];
            Assert.IsFalse(slot.IsWorking, "Job should not start in wrong machine type.");
        }

        [Test]
        public void TestSlotExpansion()
        {
            // Unlock Smelter
            workshopManager.UnlockSmelter(true);
            Assert.AreEqual(3, workshopManager.Slots.Count); // 2 init + 1 new
            Assert.AreEqual(2, workshopManager.SmelterCount);
            Assert.AreEqual(MachineType.BasicSmelter, workshopManager.Slots[2].InstalledMachine);

            // Unlock Max
            workshopManager.UnlockSmelter(true); // 3
            workshopManager.UnlockSmelter(true); // 4
            workshopManager.UnlockSmelter(true); // 5
            workshopManager.UnlockSmelter(true); // 6 (Should fail/ignore)

            Assert.AreEqual(5, workshopManager.SmelterCount);
        }

        [Test]
        public void TestJobCompletion()
        {
            ResourceManager.Instance.AddResource(ResourceType.Iron, 100);
            workshopManager.StartJob(0, "SMELT_STEEL");

            var slot = workshopManager.Slots[0];
            MethodInfo tick = workshopManager.GetType().GetMethod("TickWorkshop", BindingFlags.Instance | BindingFlags.NonPublic);

            // 6 ticks for 5s duration
            for(int i=0; i<6; i++) tick.Invoke(workshopManager, null);

            Assert.IsFalse(slot.IsWorking);
            Assert.AreEqual(1, ResourceManager.Instance.GetResourceQuantity(ResourceType.Steel));
        }
    }
}
