using NUnit.Framework;
using UnityEngine;
using SpaceRush.Systems;
using SpaceRush.Models;
using SpaceRush.Core;

namespace SpaceRush.Tests
{
    public class FleetManagerTests
    {
        private GameObject gameObj;

        [SetUp]
        public void Setup()
        {
            gameObj = new GameObject("GameManager");

            // Dependencies
            // GameDatabase
            gameObj.AddComponent<GameDatabase>();

            // ResourceManager (needed for ResearchManager hiring/investing)
            gameObj.AddComponent<ResourceManager>();

            // ResearchManager
            var rm = gameObj.AddComponent<ResearchManager>();
            rm.SendMessage("Start"); // Initialize Tech Tree

            // FleetManager
            gameObj.AddComponent<FleetManager>();
        }

        [TearDown]
        public void Teardown()
        {
            UnityEngine.Object.DestroyImmediate(gameObj);
        }

        [Test]
        public void Test_InitialStats()
        {
            Assert.AreEqual(1, FleetManager.Instance.ShipLevel);
            Assert.AreEqual(1.0f, FleetManager.Instance.MiningSpeed);
            Assert.AreEqual(10, FleetManager.Instance.CargoCapacity);
        }

        [Test]
        public void Test_UpgradeShip()
        {
            // Repair ship first (required for upgrade)
            FleetManager.Instance.RepairShip(100f);
            Assert.IsTrue(FleetManager.Instance.IsOperational);

            // Upgrade
            FleetManager.Instance.UpgradeShip();

            Assert.AreEqual(2, FleetManager.Instance.ShipLevel);
            Assert.AreEqual(20, FleetManager.Instance.CargoCapacity);
            // Speed: 1.0 * 1.2^(2-1) = 1.2
            Assert.AreEqual(1.2f, FleetManager.Instance.MiningSpeed, 0.001f);
        }

        [Test]
        public void Test_RecalculateStats_WithTech()
        {
            // Grant Credits
            ResourceManager.Instance.SetCredits(100000);

            // Invest to get RP
            ResearchManager.Instance.InvestInResearch(10000); // 1000 RP

            // Unlock "EFFICIENCY_1" (Cost 100 RP)
            ResearchManager.Instance.UnlockTechnology("EFFICIENCY_1");

            Assert.IsTrue(ResearchManager.Instance.IsTechUnlocked("EFFICIENCY_1"));

            // Check Fleet Stats (should be updated by UnlockTechnology automatically)
            // Base Speed (Lvl 1) = 1.0. With Tech (+10%) = 1.1.
            Assert.AreEqual(1.1f, FleetManager.Instance.MiningSpeed, 0.001f);
        }

        [Test]
        public void Test_RepairShip()
        {
            Assert.IsFalse(FleetManager.Instance.IsOperational);
            FleetManager.Instance.RepairShip(50f);
            Assert.AreEqual(50f, FleetManager.Instance.RepairStatus);
            Assert.IsFalse(FleetManager.Instance.IsOperational);

            FleetManager.Instance.RepairShip(50f);
            Assert.AreEqual(100f, FleetManager.Instance.RepairStatus);
            Assert.IsTrue(FleetManager.Instance.IsOperational);
        }
    }
}
