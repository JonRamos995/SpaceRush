using NUnit.Framework;
using UnityEngine;
using SpaceRush.Systems;
using SpaceRush.Core;
using SpaceRush.Data;
using SpaceRush.Models;
using System.Collections.Generic;

namespace ShadowTests
{
    [TestFixture]
    public class ResearchTests
    {
        [SetUp]
        public void Setup()
        {
            // Setup Singletons
            var go = new GameObject("GameManager");
            var gm = go.AddComponent<GameManager>();

            // Manually add GameDatabase since GM.Start isn't called automatically
            var db = go.AddComponent<GameDatabase>();

            // Initialize ResourceManager for credits
            go.AddComponent<ResourceManager>();

            // Initialize ResearchManager
            var rm = go.AddComponent<ResearchManager>();

            // Trigger initializing tech tree manually because GameDatabase might be empty initially
            // and we want to inject our test tech.
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(GameManager.Instance);
        }

        [Test]
        public void TestResearchApplyEffect()
        {
            // 1. Setup - Create a fake tech with an effect
            var tech = new TechDefinition("TEST_TECH", "Test Tech", "Desc", 100, 10);

            // Create Effect
            var effect = ScriptableObject.CreateInstance<StatBonusEffect>();
            effect.StatName = "MiningSpeed";
            effect.Modifier = 0.5f;

            tech.Effects.Add(effect);

            // 2. Inject into Database
            // GameDatabase loads automatically in Awake.
            GameDatabase.Instance.Technologies.Add(tech);

            // Re-init ResearchManager to pick it up
            ResearchManager.Instance.SendMessage("InitializeTechTree");

            // 3. Unlock it
            ResourceManager.Instance.AddCredits(10000);
            ResearchManager.Instance.InvestInResearch(200); // 200 credits => 20 RP

            ResearchManager.Instance.UnlockTechnology("TEST_TECH");

            // 4. Verify
            Assert.IsTrue(ResearchManager.Instance.IsTechUnlocked("TEST_TECH"));
            float bonus = ResearchManager.Instance.GetStatBonus("MiningSpeed");
            Assert.AreEqual(0.5f, bonus, 0.001f);
        }

        [Test]
        public void TestUnlockFeatureEffect()
        {
            var tech = new TechDefinition("FEATURE_TECH", "Feature Tech", "Desc", 100, 10);
            var effect = ScriptableObject.CreateInstance<UnlockFeatureEffect>();
            effect.FeatureID = "AUTO_REPAIR";
            tech.Effects.Add(effect);

            GameDatabase.Instance.Technologies.Add(tech);
            ResearchManager.Instance.SendMessage("InitializeTechTree");

            ResourceManager.Instance.AddCredits(1000);
            ResearchManager.Instance.InvestInResearch(200);
            ResearchManager.Instance.UnlockTechnology("FEATURE_TECH");

            Assert.IsTrue(ResearchManager.Instance.IsFeatureUnlocked("AUTO_REPAIR"));
        }
    }
}
