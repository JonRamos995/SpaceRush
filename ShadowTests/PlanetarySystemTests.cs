using NUnit.Framework;
using UnityEngine;
using SpaceRush.Systems;
using SpaceRush.Models;
using SpaceRush.Core;
using SpaceRush.Data;
using System.Collections.Generic;

namespace SpaceRush.Tests
{
    public class PlanetarySystemTests
    {
        private GameObject gameObj;

        [SetUp]
        public void Setup()
        {
            if (GameManager.Instance != null) Object.DestroyImmediate(GameManager.Instance.gameObject);
            if (GameDatabase.Instance != null) Object.DestroyImmediate(GameDatabase.Instance.gameObject);
            if (ResourceManager.Instance != null) Object.DestroyImmediate(ResourceManager.Instance.gameObject);
            if (LocationManager.Instance != null) Object.DestroyImmediate(LocationManager.Instance.gameObject);

            gameObj = new GameObject("GameManager");
            gameObj.AddComponent<GameDatabase>();
            gameObj.AddComponent<ResourceManager>();

            var rm = gameObj.AddComponent<ResearchManager>();
            rm.SendMessage("Start");

            var locMan = gameObj.AddComponent<LocationManager>();
            locMan.SendMessage("Start");

            gameObj.AddComponent<PlanetarySystem>();
        }

        [TearDown]
        public void Teardown()
        {
            UnityEngine.Object.DestroyImmediate(gameObj);
        }

        [Test]
        public void Test_ProduceResources_Success()
        {
            // Get a location (Moon)
            var loc = LocationManager.Instance.Locations.Find(l => l.ID == "MOON");
            Assert.IsNotNull(loc, "Moon location not found");

            // Setup State
            loc.State = DiscoveryState.ReadyToMine;
            loc.Infrastructure.MiningLevel = 1;
            loc.Infrastructure.StationLevel = 1; // Capacity 100
            loc.Stockpile.Clear();

            // Act
            PlanetarySystem.Instance.ProduceResources(loc);

            // Assert
            // Moon has Iron.
            Assert.IsTrue(loc.Stockpile.ContainsKey(ResourceType.Iron));
            Assert.AreEqual(1, loc.Stockpile[ResourceType.Iron]);
        }

        [Test]
        public void Test_ProduceResources_CapacityFull()
        {
            var loc = LocationManager.Instance.Locations.Find(l => l.ID == "MOON");
            loc.State = DiscoveryState.ReadyToMine;
            loc.Infrastructure.MiningLevel = 1;
            loc.Infrastructure.StationLevel = 1; // Capacity 100

            // Fill Stockpile
            loc.Stockpile[ResourceType.Iron] = 100;

            // Act
            PlanetarySystem.Instance.ProduceResources(loc);

            // Assert
            Assert.AreEqual(100, loc.Stockpile[ResourceType.Iron]);
        }

        [Test]
        public void Test_StartMiningOperations()
        {
            // Pick a location that needs investigation (Mars)
            var loc = LocationManager.Instance.Locations.Find(l => l.ID == "MARS");
            loc.State = DiscoveryState.Investigated;

            // Ensure Tech is unlocked
            // Mars needs ENV_SUIT_MK2
            ResourceManager.Instance.SetCredits(100000);
            ResearchManager.Instance.InvestInResearch(10000); // 1000 RP
            ResearchManager.Instance.UnlockTechnology("ENV_SUIT_MK2"); // Cost 500

            // Act
            PlanetarySystem.Instance.StartMiningOperations("MARS");

            // Assert
            Assert.AreEqual(DiscoveryState.ReadyToMine, loc.State);
            Assert.AreEqual(1, loc.Infrastructure.MiningLevel);
        }
    }
}
