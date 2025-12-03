using NUnit.Framework;
using UnityEngine;
using SpaceRush.Core;
using SpaceRush.Systems;
using SpaceRush.Models;
using SpaceRush.Data;
using System.Collections.Generic;
using System.Reflection;

namespace ShadowTests
{
    [TestFixture]
    public class BiomeBonusTests
    {
        private PlanetarySystem planetarySystem;
        private LocationManager locationManager;
        private ResearchManager researchManager;

        [SetUp]
        public void Setup()
        {
             // Cleanup
            if (GameManager.Instance != null) Object.DestroyImmediate(GameManager.Instance.gameObject);
            if (GameDatabase.Instance != null) Object.DestroyImmediate(GameDatabase.Instance.gameObject);
            if (ResourceManager.Instance != null) Object.DestroyImmediate(ResourceManager.Instance.gameObject);
            if (PlanetarySystem.Instance != null) Object.DestroyImmediate(PlanetarySystem.Instance.gameObject);
            if (LocationManager.Instance != null) Object.DestroyImmediate(LocationManager.Instance.gameObject);
            if (ResearchManager.Instance != null) Object.DestroyImmediate(ResearchManager.Instance.gameObject);

            // Dependencies
            var dbGo = new GameObject("GameDatabase");
            dbGo.AddComponent<GameDatabase>();

            var resGo = new GameObject("ResourceManager");
            resGo.AddComponent<ResourceManager>();

            var rmGo = new GameObject("ResearchManager");
            researchManager = rmGo.AddComponent<ResearchManager>();
            MethodInfo rmStart = researchManager.GetType().GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic);
            if (rmStart != null) rmStart.Invoke(researchManager, null);

            var fmGo = new GameObject("FleetManager");
            fmGo.AddComponent<FleetManager>();

            var lmGo = new GameObject("LocationManager");
            locationManager = lmGo.AddComponent<LocationManager>();
            MethodInfo lmStart = locationManager.GetType().GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic);
            if (lmStart != null) lmStart.Invoke(locationManager, null);

            var psGo = new GameObject("PlanetarySystem");
            planetarySystem = psGo.AddComponent<PlanetarySystem>();
        }

        [TearDown]
        public void TearDown()
        {
            if (GameDatabase.Instance != null) Object.DestroyImmediate(GameDatabase.Instance.gameObject);
            if (PlanetarySystem.Instance != null) Object.DestroyImmediate(PlanetarySystem.Instance.gameObject);
            if (LocationManager.Instance != null) Object.DestroyImmediate(LocationManager.Instance.gameObject);
            if (ResearchManager.Instance != null) Object.DestroyImmediate(ResearchManager.Instance.gameObject);
            if (FleetManager.Instance != null) Object.DestroyImmediate(FleetManager.Instance.gameObject);
        }

        [Test]
        public void TestBiomeBonus_Barren()
        {
            // Find Moon (Barren)
            var moon = locationManager.Locations.Find(l => l.Definition.Biome == BiomeType.Barren);
            Assert.IsNotNull(moon, "Moon/Barren location not found");

            // Setup infrastructure
            moon.Infrastructure.MiningLevel = 100; // Base 100 production
            moon.Stockpile.Clear();

            // 1. Without Tech
            planetarySystem.ProduceResources(moon);

            // Should be 100 (assuming no other bonuses)
            // Definition of Moon: "AvailableResources": [0] (Iron)
            if (!moon.Stockpile.ContainsKey(ResourceType.Iron))
            {
                // If produce failed or RNG picked something else? Moon only has Iron.
                Assert.Fail("No resources produced on Moon");
            }

            var amount = moon.Stockpile[ResourceType.Iron];
            Assert.AreEqual(100, amount, "Base production mismatch");

            // 2. Unlock ENV_SUIT_MK2
            moon.Stockpile.Clear();

            // Give RP
            FieldInfo rpField = researchManager.GetType().GetField("<ResearchPoints>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            rpField.SetValue(researchManager, 10000f);

            // Unlock
            researchManager.UnlockTechnology("ENV_SUIT_MK2");
            Assert.IsTrue(researchManager.IsTechUnlocked("ENV_SUIT_MK2"));

            // 3. Produce with Bonus
            planetarySystem.ProduceResources(moon);

            amount = moon.Stockpile[ResourceType.Iron];
            // Expected: 100 * (1.0 + 0.2) = 120
            Assert.AreEqual(120, amount, "Biome bonus not applied");
        }

        [Test]
        public void TestEfficiencyBonus()
        {
             var moon = locationManager.Locations.Find(l => l.Definition.Biome == BiomeType.Barren);
             moon.Infrastructure.MiningLevel = 100;
             moon.Stockpile.Clear();

             // Give RP
             FieldInfo rpField = researchManager.GetType().GetField("<ResearchPoints>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
             rpField.SetValue(researchManager, 10000f);

             researchManager.UnlockTechnology("EFFICIENCY_1");

             planetarySystem.ProduceResources(moon);

             // Expected: 100 * 1.1 = 110
             Assert.AreEqual(110, moon.Stockpile[ResourceType.Iron]);
        }
    }
}
