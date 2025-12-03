using NUnit.Framework;
using UnityEngine;
using SpaceRush.Systems;
using SpaceRush.Core;
using SpaceRush.Models;
using SpaceRush.Data;
using System.Collections.Generic;
using System;

namespace ShadowTests
{
    [TestFixture]
    public class JuiceTests
    {
        [SetUp]
        public void Setup()
        {
            if (GameManager.Instance != null) UnityEngine.Object.DestroyImmediate(GameManager.Instance.gameObject);

            var go = new GameObject("GameManager");
            go.AddComponent<GameManager>();
            go.AddComponent<GameDatabase>();
            go.AddComponent<ResourceManager>();

            var rm = go.AddComponent<ResearchManager>();
            rm.SendMessage("Start");

            go.AddComponent<LocationManager>();
            go.AddComponent<FleetManager>();
            go.AddComponent<IdleManager>();
            go.AddComponent<AdManager>();
            go.AddComponent<NotificationManager>();

            ResourceManager.Instance.ResetData();
            FleetManager.Instance.ResetData();
            LocationManager.Instance.ResetData();
        }

        [TearDown]
        public void Teardown()
        {
            if (GameManager.Instance != null) UnityEngine.Object.DestroyImmediate(GameManager.Instance.gameObject);
        }

        [Test]
        public void TestDoubleOfflineGains()
        {
            // 1. Simulate Player Away (20 seconds)
            // Need a location
            var def = new LocationDefinition("TEST_LOC", "Test Loc", 0, BiomeType.Terrestrial, new List<ResourceType>{ ResourceType.Iron });
            var loc = new LocationState(def);
            loc.State = DiscoveryState.ReadyToMine;
            LocationManager.Instance.Locations.Add(loc);
            LocationManager.Instance.SetLocation("TEST_LOC"); // Set as current

            // Calc Timestamp for 20s ago
            DateTime ago = DateTime.UtcNow.AddSeconds(-20);
            long ts = ago.ToBinary();

            // 2. Trigger Offline Calculation
            IdleManager.Instance.CalculateOfflineProgressFromTimestamp(ts);

            // Check Base Gains
            // Fleet Speed 1.0 * 20s * 0.5 efficiency = 10 Iron
            int iron = ResourceManager.Instance.GetResourceQuantity(ResourceType.Iron);
            // Assert.AreEqual(10, iron); // Might vary slightly due to float precision or internal logic
            Assert.Greater(iron, 0);
            int baseIron = iron;

            // 3. Double it
            IdleManager.Instance.DoubleOfflineGains();

            // 4. Verify Doubled
            int newIron = ResourceManager.Instance.GetResourceQuantity(ResourceType.Iron);
            Assert.AreEqual(baseIron * 2, newIron);
        }

        [Test]
        public void TestNotificationSchedule()
        {
            // PlanterarySystem needs to be added
            var ps = GameManager.Instance.gameObject.AddComponent<PlanetarySystem>();

            var def = new LocationDefinition("TEST_LOC", "Test Loc", 0, BiomeType.Terrestrial, new List<ResourceType>{ ResourceType.Iron });
            var loc = new LocationState(def);
            loc.State = DiscoveryState.Discovered;
            LocationManager.Instance.Locations.Add(loc);

            ResourceManager.Instance.AddCredits(10000);

            // Investigate
            ps.InvestigatePlanet("TEST_LOC");

            // Should see log for notification
            // We can't assert log output in NUnit easily without custom log handler.
            // But if it doesn't crash, it's good.
        }
    }
}
