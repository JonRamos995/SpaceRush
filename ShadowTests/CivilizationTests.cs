using NUnit.Framework;
using UnityEngine;
using SpaceRush.Systems;
using SpaceRush.Core;
using SpaceRush.Models;
using SpaceRush.Data;
using System.Collections.Generic;

namespace ShadowTests
{
    [TestFixture]
    public class CivilizationTests
    {
        [SetUp]
        public void Setup()
        {
            if (GameManager.Instance != null) Object.DestroyImmediate(GameManager.Instance.gameObject);
            if (CivilizationManager.Instance != null) Object.DestroyImmediate(CivilizationManager.Instance.gameObject);

            var go = new GameObject("GameManager");
            go.AddComponent<GameManager>();
            go.AddComponent<GameDatabase>(); // Awake loads DB

            go.AddComponent<ResourceManager>(); // Awake inits resources

            var rm = go.AddComponent<ResearchManager>();
            rm.SendMessage("Start"); // Initialize Tech Tree

            go.AddComponent<CivilizationManager>();
            go.AddComponent<PersistenceManager>();
            go.AddComponent<FleetManager>();
            go.AddComponent<LocationManager>();

            // Re-init Logic
            if (CivilizationManager.Instance != null) CivilizationManager.Instance.LoadData(null);
        }

        [TearDown]
        public void Teardown()
        {
            if (GameManager.Instance != null)
            {
                Object.DestroyImmediate(GameManager.Instance.gameObject);
            }
        }

        [Test]
        public void TestAscensionGains()
        {
            CivilizationManager.Instance.LoadData(null);
            Assert.AreEqual(0, CivilizationManager.Instance.Level);
            Assert.AreEqual(0, CivilizationManager.Instance.Nanites);

            CivilizationManager.Instance.Ascend();

            Assert.AreEqual(1, CivilizationManager.Instance.Level);
            Assert.AreEqual(100f, CivilizationManager.Instance.Nanites);
        }

        [Test]
        public void TestMetaUpgradePurchase()
        {
            CivilizationManager.Instance.LoadData(null);

            // Inject Nanites
            CivilizationManager.Instance.Ascend(); // Level 1, 100 Nanites

            // Initial Multiplier (1.0 + 0.1) = 1.1
            Assert.AreEqual(1.1f, CivilizationManager.Instance.BaseMultiplier, 0.001f);

            bool success = CivilizationManager.Instance.BuyUpgrade("GLOBAL_SPEED_1");
            Assert.IsTrue(success);

            Assert.AreEqual(0, CivilizationManager.Instance.Nanites); // Spent 100

            float globalMult = CivilizationManager.Instance.GetGlobalMultiplier("GlobalMiningSpeed");
            Assert.AreEqual(1.6f, globalMult, 0.001f);
        }

        [Test]
        public void TestRetentionOnReset()
        {
            CivilizationManager.Instance.LoadData(null);

            // 1. Gain Resources
            ResourceManager.Instance.AddResource(ResourceType.Iron, 1000);

            // 2. Buy Retention Upgrade
            CivilizationManager.Instance.Ascend(); // Level 1
            bool success = CivilizationManager.Instance.BuyUpgrade("RETENTION_1");
            Assert.IsTrue(success);

            // 3. Add Iron AGAIN because Ascend Reset wiped it
            ResourceManager.Instance.AddResource(ResourceType.Iron, 1000);

            CivilizationManager.Instance.Ascend(); // Level 2

            // 4. Verify Resources
            int iron = ResourceManager.Instance.GetResourceQuantity(ResourceType.Iron);
            Assert.AreEqual(100, iron);

            Assert.AreEqual(0.1f, CivilizationManager.Instance.GetRetentionPercentage());
        }
    }
}
