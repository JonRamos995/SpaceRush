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
    public class SynergyTests
    {
        [SetUp]
        public void Setup()
        {
            if (GameManager.Instance != null) Object.DestroyImmediate(GameManager.Instance.gameObject);
            if (CivilizationManager.Instance != null) Object.DestroyImmediate(CivilizationManager.Instance.gameObject);

            var go = new GameObject("GameManager");
            go.AddComponent<GameManager>();
            go.AddComponent<GameDatabase>();
            go.AddComponent<ResourceManager>();

            var rm = go.AddComponent<ResearchManager>();
            rm.SendMessage("Start");

            go.AddComponent<LocationManager>();
            go.AddComponent<PlanetarySystem>();
            go.AddComponent<CivilizationManager>(); // Needed for bonus

            ResourceManager.Instance.ResetData();
            LocationManager.Instance.ResetData();
        }

        [TearDown]
        public void Teardown()
        {
            if (GameManager.Instance != null) Object.DestroyImmediate(GameManager.Instance.gameObject);
        }

        [Test]
        public void TestSynergyConsumptionAndBoost()
        {
            // 1. Setup Location
            var def = new LocationDefinition();
            def.ID = "SYNERGY_PLANET";
            def.AvailableResources = new List<ResourceType> { ResourceType.Iron };
            def.HasSynergy = true;
            def.SynergyResource = ResourceType.Ice;
            def.SynergyMultiplier = 1.0f; // +100%

            var loc = new LocationState(def);
            loc.State = DiscoveryState.ReadyToMine;
            loc.Infrastructure.MiningLevel = 10; // Base 10
            loc.Infrastructure.StationLevel = 1;

            // Register in Manager
            LocationManager.Instance.Locations.Add(loc);

            // 2. Scenario A: No Ice in Global Stockpile
            // Expected: Base Production (10) * CivMulti (1.0) = 10

            PlanetarySystem.Instance.ProduceResources(loc);

            int produced = loc.Stockpile[ResourceType.Iron];
            Assert.AreEqual(10, produced);

            // Reset Stockpile
            loc.Stockpile.Clear();

            // 3. Scenario B: Add Ice
            ResourceManager.Instance.AddResource(ResourceType.Ice, 10);

            PlanetarySystem.Instance.ProduceResources(loc);

            // Expected: (10) * (1.0 + 1.0) = 20
            produced = loc.Stockpile[ResourceType.Iron];
            Assert.AreEqual(20, produced);

            // Verify Ice Consumed
            int ice = ResourceManager.Instance.GetResourceQuantity(ResourceType.Ice);
            Assert.AreEqual(9, ice);
        }
    }
}
