using NUnit.Framework;
using UnityEngine;
using SpaceRush.Systems;
using SpaceRush.Core;
using SpaceRush.Models;
using SpaceRush.Data;

namespace ShadowTests
{
    [TestFixture]
    public class FleetTests
    {
        [SetUp]
        public void Setup()
        {
            if (GameManager.Instance != null) Object.DestroyImmediate(GameManager.Instance.gameObject);

            var go = new GameObject("GameManager");
            go.AddComponent<GameManager>();
            go.AddComponent<GameDatabase>();
            go.AddComponent<ResourceManager>();

            var rm = go.AddComponent<ResearchManager>();
            rm.SendMessage("Start");

            go.AddComponent<FleetManager>();

            ResearchManager.Instance.ResetData();
            FleetManager.Instance.ResetData();
            ResourceManager.Instance.ResetData();
        }

        [TearDown]
        public void Teardown()
        {
            if (GameManager.Instance != null) Object.DestroyImmediate(GameManager.Instance.gameObject);
        }

        [Test]
        public void TestAutoRepair()
        {
            Assert.IsFalse(FleetManager.Instance.IsOperational);

            // Unlock Feature
            ResearchManager.Instance.UnlockFeature("REPAIR_DROID");

            ResourceManager.Instance.AddCredits(1000);

            // Simulate 1.1 seconds
            for(int i=0; i<11; i++)
            {
                FleetManager.Instance.SendMessage("Update");
            }

            Assert.AreEqual(5.0f, FleetManager.Instance.RepairStatus, 0.01f);
            Assert.AreEqual(990f, ResourceManager.Instance.Credits);
        }
    }
}
